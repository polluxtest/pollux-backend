namespace Pollux.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.AspNetCore.AccessTokenManagement;
    using IdentityServer4.Models;
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
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Domain.Entities;
    using Pollux.Persistence;
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
                .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
                .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
                .AddAspNetIdentity<User>()
                .AddDeveloperSigningCredential()
                .AddResourceOwnerValidator<UserValidator>();

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
                });

            services.AddMvc();
            services.AddAuthorization();
            services.AddAccessTokenManagement();
            services.AddControllers();
            services.AddSwaggerGen();
            this.SetUpSwagger(services);
            services.AddDIRepositories();
            services.AddDIServices();
            services.AddIdentityServerServices();
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

                          ServiceProvider serviceProvider = services.BuildServiceProvider();
                          var redisCacheService = serviceProvider.GetService<IRedisCacheService>();
                          var tokenEndpointServie = serviceProvider.GetService<ITokenEndpointService>();
                          var loggedUserEmail = context.Principal.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email).Value;
                          var token = await redisCacheService.GetObjectAsync<TokenModel>(loggedUserEmail);

                          context.Request.Headers.TryGetValue("Authorization", out var authValues);
                          var authotizationToken = authValues.First();

                          if (!token.AccessToken.Equals(authotizationToken) ||
                                  !context.Principal.Identity.IsAuthenticated ||
                                  string.IsNullOrEmpty(token.AccessToken) ||
                                  DateTime.UtcNow > token.RefreshTokenExpirationDate)
                          {
                              context.RejectPrincipal();
                          }

                          if (DateTime.UtcNow > token.AccessTokenExpirationDate)
                          {
                              var newAccesstokenResponse = await tokenEndpointServie.RefreshUserAccessTokenAsync(token.RefreshToken);
                              token.AccessToken = newAccesstokenResponse.AccessToken;
                              token.AccessTokenExpirationDate = DateTime.UtcNow.AddSeconds(5); // todo definw expiration and join code
                              var success = await redisCacheService.SetObjectAsync<TokenModel>(loggedUserEmail, token, TimeSpan.FromDays(7));
                              if (success)
                              {

                              }
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

        /// <summary
        /// Sets up authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        private void AddRedisCacheService(IServiceCollection services)
        {
            services.AddTransient<IRedisCacheService, RedisCacheService>();
        }
    }
}
