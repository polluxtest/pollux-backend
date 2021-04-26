namespace Pollux.API
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
    using Pollux.Application.Mappers;
    using Pollux.Application.OAuth;
    using Pollux.Common.Application.Models.Settings;
    using Pollux.Common.Constants.Strings;
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
            var connectionString = this.Configuration.GetSection("AppSettings")["DbConnectionStrings:PolluxSQLConnectionString"];
            var identityServerSettings = new IdentityServerSettings();
            this.Configuration.Bind("IdentityServerSettings", identityServerSettings);
            services.AddSingleton(identityServerSettings);
            services.AddDbContext<PolluxDbContext>(options => options.UseSqlServer(connectionString));
            services.AddIdentityCore<User>().AddEntityFrameworkStores<PolluxDbContext>().AddDefaultTokenProviders();
            this.SetUpPasswordIdentity(services);
            services.AddCors();
            this.SetUpIdentityServer(services);
            this.SetUpAuthentication(services, identityServerSettings.HostUrl);
            services.AddMvc();
            services.AddAuthorization();
            services.AddDIClientAccessTokenManagement();
            services.AddDIMiscelaneus();
            services.AddControllers();
            services.AddSwaggerGen();
            this.SetUpSwagger(services);
            services.AddDIRepositories();
            services.AddDIServices();
            services.AddDIIdentityServerServices();
            services.AddAutoMapper(AssemblyApplication.Assembly);
            this.SetUpCookieAndHandler(services);
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

            this.AddSwagger(app);
            app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
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
                                        Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer"},
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
        private void SetUpCookieAndHandler(IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = CookiesConstants.CookieSessionName;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.HttpContext.User.Identity.IsAuthenticated)
                    {
                        return Task.CompletedTask;
                    }

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

                options.Events.OnValidatePrincipal = async context =>
                {
                    await new AuthEventHandler(services).Handle(context);
                };
            });
        }

        private void SetUpAuthentication(IServiceCollection services, string identityServerUrl)
        {
            services.AddAuthentication(
               options =>
               {
                   options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               })
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
        private void SetUpIdentityServer(IServiceCollection services)
        {
            services.AddIdentityServer(
                options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;
                })
             .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
             .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
             .AddAspNetIdentity<User>()
             .AddDeveloperSigningCredential()
             .AddResourceOwnerValidator<UserValidator>();
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
    }

}
