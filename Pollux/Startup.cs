namespace Pollux.API
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using IdentityModel.Client;

    using IdentityServer4.Models;
    using IdentityServer4.Test;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
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

    using Pollux.Application.Mappers;
    using Pollux.Application.OAuth;
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
            var connectionString =
                this.Configuration.GetSection("AppSettings")["DbConnectionStrings:PolluxSQLConnectionString"];


            services.AddDbContext<PolluxDbContext>(options => options.UseSqlServer(connectionString));

            services.AddIdentityCore<User>().AddEntityFrameworkStores<PolluxDbContext>().AddDefaultTokenProviders();

            IdentityModelEventSource.ShowPII = true;

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
                .AddTestUsers(new List<TestUser>() { new TestUser() { Username = "octa@gmail.com", Password = "apolo100" } })
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddAspNetIdentity<User>()
                .AddDeveloperSigningCredential().AddResourceOwnerValidator<UserValidator>()
                .AddCustomTokenRequestValidator<TokenValidator>().AddProfileService<ProfileService>();


            services.Configure<IdentityOptions>(options =>
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequiredLength = 8;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                    });
            var tokenValidationParameters = new TokenValidationParameters()
            {

                // Clock skew compensates for server time drift.
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = true,
                ValidAudience = "api://default",
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false
            };
            services.AddCors();

            services.AddAuthentication(
                options =>
                    {
                        options.DefaultScheme = "Cookies";
                        options.DefaultChallengeScheme = "oidc";
                    }).AddCookie("Cookies").AddOpenIdConnect(
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
                        options.TokenValidationParameters = tokenValidationParameters;
                    });

            services.AddAuthorization();
            services.AddMvc();
            services.AddAccessTokenManagement();
            services.AddControllers();

            services.AddSwaggerGen();
            this.SetUpSwagger(services);
            services.AddDIRepositories();
            services.AddDIServices();
            services.AddAutoMapper(AssemblyApplication.Assembly);



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
            app.UseHttpsRedirection();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); }); // add require auth
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

        /// <summary>
        /// Sets up identity server.
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetUpIdentityServer(IServiceCollection services)
        {
            //var builder = 
        }

        /// <summary
        /// Sets up authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetUpAuthentication(IServiceCollection services)
        {
            services.AddAuthentication();
        }
    }
}
