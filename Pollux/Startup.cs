namespace Pollux.API
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Pollux.Common.ExtensionMethods;
    using IdentityModel.Client;

    using IdentityServer4.Models;
    using IdentityServer4.Test;
    using Microsoft.AspNetCore.Authentication;
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
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.Application.Mappers;
    using Pollux.Application.OAuth;
    using Pollux.Domain.Entities;
    using Pollux.Persistence;
    using IdentityModel.AspNetCore.AccessTokenManagement;
    using System.Security.Claims;
    using StackExchange.Redis;
    using Pollux.Persistence.Services;
    using Pollux.Persistence.Services.Cache;




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
            var connectionString = this.Configuration.GetSection("AppSettings")["DbConnectionStrings:PolluxSQLConnectionString"];

            services.AddDbContext<PolluxDbContext>(options => options.UseSqlServer(connectionString));

            services.AddIdentityCore<User>().AddEntityFrameworkStores<PolluxDbContext>().AddDefaultTokenProviders();

            IdentityModelEventSource.ShowPII = true;

            services.Configure<IdentityOptions>(options =>
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequiredLength = 8;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                    });

            services.AddCors();

            services.AddIdentityServer(
                    options =>
                        {
                            options.Events.RaiseErrorEvents = true;
                            options.Events.RaiseInformationEvents = true;
                            options.Events.RaiseFailureEvents = true;
                            options.Events.RaiseSuccessEvents = true;
                            options.EmitStaticAudienceClaim = true;
                        })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddTestUsers(new List<TestUser>() { new TestUser() { Username = "octa@gmail.com", Password = "apolo100" },
                                new TestUser() { Username = "octavio.diaz@gmail.com", Password = "apolo100" },})
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddAspNetIdentity<User>()
                .AddDeveloperSigningCredential().AddResourceOwnerValidator<UserValidator>()
                .AddCustomTokenRequestValidator<TokenValidator>();

            services.AddAuthentication(
                options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                    }).AddOpenIdConnect(
                "oidc",
                options =>
                    {
                        options.Authority = "http://localhost:5000/connect/token";
                        options.ClientId = "client";
                        options.ClientSecret = "secret".Sha256();
                        options.SaveTokens = true;
                        options.Configuration = new OpenIdConnectConfiguration() { };
                        options.Scope.Add("api");
                        options.Scope.Add("offline_access");
                        //options.TokenValidationParameters = tokenValidationParameters;
                    });



            services.AddMvc();
            services.AddAuthorization();
            services.AddAccessTokenManagement();
            services.AddControllers();
            services.AddSwaggerGen();
            this.SetUpSwagger(services);
            services.AddDIRepositories();
            services.AddDIServices();
            services.AddAutoMapper(AssemblyApplication.Assembly);
            this.AddRedisCacheService(services);

            services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.Name = "auth_cookie";
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.LoginPath = new PathString("/api/pollux/User/LogIn/");
                    options.AccessDeniedPath = new PathString("/api/pollux/User/Denied/");

                    // Not creating a new object since ASP.NET Identity has created
                    // one already and hooked to the OnValidatePrincipal event.
                    // See https://github.com/aspnet/AspNetCore/blob/5a64688d8e192cacffda9440e8725c1ed41a30cf/src/Identity/src/Identity/IdentityServiceCollectionExtensions.cs#L56
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
                        if (context.Request.Path.Equals("/api/pollux/User/LogIn"))
                        {
                            return;
                        }

                        var cookie = context.Request.Cookies.FirstOrDefault(p => p.Key.Equals("auth_cookie"));

                        var serviceProvider = services.BuildServiceProvider();
                        var redisCacheService = serviceProvider.GetService<IRedisCacheService>();
                        var tokenEndpointServie = serviceProvider.GetService<ITokenEndpointService>();
                        var tokenCache = await redisCacheService.GetKeyAsync(context.Principal.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email).Value);
                        var token = tokenCache.DecodeToken();

                        //check access token expiration in UTC
                        //check refresh token expiration in UTC
                        //implement refresh token functionallity
                        // implement cookie authorization with tokens
                        context.Request.Headers.TryGetValue("Authorization", out var authValues);
                        var authotizationToken = authValues.First();

                        if (!token.AccessToken.Equals(authotizationToken) ||
                                !context.Principal.Identity.IsAuthenticated ||
                                string.IsNullOrEmpty(token.AccessToken))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.RejectPrincipal();
                        }

                        if (DateTime.UtcNow > token.ExpirationDate)
                        {
                            var newAccesstokenResponse = await tokenEndpointServie.RefreshUserAccessTokenAsync(token.RefreshToken);

                        }

                        return;
                    };
                });
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
                    }); // add require auth
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

        private void SetUpSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
                {

                    c.AddSecurityDefinition(
                        "Bearer",
                        new OpenApiSecurityScheme
                        {
                            Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.ApiKey,
                            Scheme = "Bearer"
                        });

                    c.AddSecurityRequirement(
                        new OpenApiSecurityRequirement()
                            {
                                {
                                    new OpenApiSecurityScheme
                                        {
                                            Reference = new OpenApiReference
                                                            {
                                                                Type = ReferenceType.SecurityScheme, Id = "Bearer"
                                                            },
                                            Scheme = "oauth2",
                                            Name = "Bearer",
                                            In = ParameterLocation.Header,
                                        },
                                    new List<string>()
                                }
                            });
                });
        }



        /// <summary
        /// Sets up authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        private void AddRedisCacheService(IServiceCollection services)
        {
            services.AddScoped<IRedisCacheService, RedisCacheService>();
        }
    }
}
