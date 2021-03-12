namespace Pollux.API
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json.Linq;
    using Pollux.Application.Mappers;
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
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            this.SetUpAuthentication(services);



            services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                });
            this.SetUpIdentityServer(services);
            this.SetUpAuthentication(services);
            services.AddControllers();
            services.AddSwaggerGen();
            ////this.SetUpSwagger(services);
            services.AddDbContext<PolluxDbContext>(options => options.UseSqlServer(connectionString));
            services.AddCors();
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
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            this.AddSwagger(app);

            app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); }); // add require auth

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
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
            services.AddSwaggerGen(
                c =>
                    {
                        // configure SwaggerDoc and others

                        // add JWT Authentication
                        var securityScheme = new OpenApiSecurityScheme
                        {
                            Name = "JWT Authentication",
                            Description = "Enter JWT Bearer token **_only_**",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer", // must be lower case
                            BearerFormat = "JWT",
                            Reference = new OpenApiReference
                            {
                                Id = JwtBearerDefaults.AuthenticationScheme,
                                Type = ReferenceType.SecurityScheme
                            }
                        };
                        c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                        c.AddSecurityRequirement(
                            new OpenApiSecurityRequirement { { securityScheme, new string[] { } } });

                        // add Basic Authentication
                        var basicSecurityScheme = new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "basic",
                            Reference = new OpenApiReference
                            {
                                Id = "BasicAuth",
                                Type = ReferenceType.SecurityScheme
                            }
                        };
                        c.AddSecurityDefinition(basicSecurityScheme.Reference.Id, basicSecurityScheme);
                        c.AddSecurityRequirement(
                            new OpenApiSecurityRequirement { { basicSecurityScheme, new string[] { } } });
                    });
        }

        /// <summary>
        /// Sets up identity server.
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetUpIdentityServer(IServiceCollection services)
        {
            var builder = services.AddIdentityServer(
                    options =>
                        {
                            options.Events.RaiseErrorEvents = true;
                            options.Events.RaiseInformationEvents = true;
                            options.Events.RaiseFailureEvents = true;
                            options.Events.RaiseSuccessEvents = true;
                            // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                            options.EmitStaticAudienceClaim = true;
                        }).AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryClients(Config.Clients);
        }

        /// <summary>
        /// Sets up authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetUpAuthentication(IServiceCollection services)
        {
            services.AddAuthentication();
        }
    }
}
