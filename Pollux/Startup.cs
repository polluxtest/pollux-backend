using Pollux.Common.Application.Settings;

namespace Pollux.API
{
    using System.Collections.Generic;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.OpenApi.Models;
    using Pollux.API.Auth;
    using Pollux.API.Auth.AuthIdentityServer;
    using Pollux.API.ExtensionMethods;
    using Pollux.API.Middlewares;
    using Pollux.Application.Mappers;
    using Pollux.Common.Constants.Strings;
    using Pollux.Common.Constants.Strings.Auth;
    using Pollux.Common.Constants.Strings.ServerLess;
    using Pollux.Domain.Entities;
    using Pollux.Persistence;

    /// <summary>
    /// Defines the <see cref="Startup" />.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration<see cref="IConfiguration"/>.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The ConfigureServices.
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/>.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            var connectionString =
                this.Configuration.GetSection("AppSettings")["DbConnectionStrings:PolluxSQLConnectionString"];
            var allowedOrigins = this.Configuration.GetSection("AppSettings")["AllowedOrigins"];
            var frontEndUrl = this.Configuration.GetSection("AppSettings")["FrontEndUrl"];
            var identityServerSettings = new IdentityServerSettings();
            this.Configuration.Bind("AppSettings:IdentityServerSettings", identityServerSettings);
            services.AddSingleton(identityServerSettings);
            services.AddDbContext<PolluxDbContext>(options => options.UseSqlServer(connectionString));
            this.SetUpAuthentication(services, identityServerSettings.HostUrl);
            services.AddIdentityCore<User>().AddEntityFrameworkStores<PolluxDbContext>().AddDefaultTokenProviders();
            services.AddIdentity<User, Role>(
                options => { options.User.RequireUniqueEmail = false; });
            this.SetUpIdentityServer(services, identityServerSettings.HostUrl);
            services.AddAuthorization();
            this.AddSession(services, frontEndUrl);
            this.SetUpPasswordIdentity(services);
            this.AddCors(services, allowedOrigins);
            services.AddMvc()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(AssemblyPresentation.Assembly));
            services.AddDIClientAccessTokenManagement();
            services.AddDIMiscellaneous();
            services.AddControllers();
            services.AddSwaggerGen();
            this.SetUpSwagger(services);
            services.AddDIRepositories();
            services.AddDIServices();
            services.AddDIIdentityServerServices();
            this.MapConfigurationAppSettings(services);
            services.AddAutoMapper(AssemblyApplication.Assembly);

            this.SetUpCookieAndHandler(services, frontEndUrl);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();
            this.AddSwagger(app);
            app.UseCors(CookiesConstants.CookiePolicy);
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseIdentityServer();
            app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller}/{action}/{id}");
            });
        }

        /// <summary>
        /// Adds the swagger.
        /// </summary>
        /// <param name="app">The application.</param>
        private void AddSwagger(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });
        }

        /// <summary>
        /// Sets up swagger.
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetUpSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = @"Bearer {access token}",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                    });

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        },
                    });
            });
        }

        /// <summary>
        /// Sets up cookie and handler.
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetUpCookieAndHandler(IServiceCollection services, string host)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = CookiesConstants.CookieSessionName;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.HttpOnly = false;
                options.Cookie.Domain = host;
                options.Cookie.IsEssential = true;
            });
        }

        /// <summary>
        /// Sets up authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="identityServerUrl">The identity server URL.</param>
        private void SetUpAuthentication(IServiceCollection services, string identityServerUrl)
        {
            services.AddAuthentication(
                    options => { options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(
                    AuthConstants.TokenAuthenticationDefaultScheme, o => { })
                .AddOpenIdConnect(
                    OpenIdConstants.SchemaName,
                    options =>
                    {
                        options.Authority = identityServerUrl;
                        options.ClientId = IdentityServerConstants.ClientName;
                        options.ClientSecret = IdentityServerConstants.ClientSecret;
                        options.SaveTokens = true;
                        options.Configuration = new OpenIdConnectConfiguration() { };
                        options.Scope.Add(IdentityServerConstants.Scope);
                        options.Scope.Add(IdentityServerConstants.RequestRefreshToken);
                    });
        }

        /// <summary>
        /// Sets up identity server.
        /// </summary>
        /// <param name="services">The services.</param>
        private void
            SetUpIdentityServer(IServiceCollection services, string identityServerUrl)
        {
            services.AddIdentityServer(
                options =>
                {
                    options.IssuerUri = identityServerUrl;
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;
                    options.Authentication.CheckSessionCookieSameSiteMode = SameSiteMode.Lax;
                    options.Authentication.CookieSameSiteMode = SameSiteMode.Lax;
                })
             .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
             .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
             .AddAspNetIdentity<User>();
        }

        /// <summary>
        /// Sets up password identity.
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetUpPasswordIdentity(IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });
        }

        /// <summary>
        /// Adds the cors policy set up.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="origin">The front end url.</param>
        private void AddCors(IServiceCollection services, string allowedOrigins)
        {
            var origins = allowedOrigins.Split(",");
            services.AddCors(options =>
            {
                options.AddPolicy(
                    CookiesConstants.CookiePolicy,
                    builder =>
                        builder.WithOrigins(origins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials());
            });
        }

        /// <summary>
        /// Adds the session.
        /// </summary>
        /// <param name="services">The services.</param>
        private void AddSession(IServiceCollection services, string host)
        {
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;
                options.Cookie.Domain = host;
            });
        }

        /// <summary>
        /// Maps the configuration application settings.
        /// </summary>
        /// <param name="services">The services.</param>
        private void MapConfigurationAppSettings(IServiceCollection services)
        {
            var azureServerlessSettings = this.Configuration.GetSection("AzureServerlessSettings")
                .Get<AzureServerlessSettings>();

            services.AddSingleton<AzureServerlessSettings>(azureServerlessSettings);
        }
    }
}
