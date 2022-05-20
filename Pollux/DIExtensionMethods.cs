using System.Security.Claims;
using Pollux.Application.Services;

namespace Pollux.API
{
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Pollux.API.AuthIdentityServer;
    using Pollux.Application;
    using Pollux.Application.Serverless;
    using Pollux.Common.Constants;
    using Pollux.Common.Constants.Strings;
    using Pollux.Domain.Entities;
    using Pollux.Persistence;
    using Pollux.Persistence.Repositories;
    using Pollux.Persistence.Services.Cache;
    using System.Net.Http;

    /// <summary>
    /// Extension Methods for DI.
    /// </summary>
    public static class DIExtensionMethods
    {
        /// <summary>
        /// Adds the di repositories as an extension methods for the startup .
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static void AddDIRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
        }

        /// <summary>
        /// Adds the di services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static void AddDIServices(this IServiceCollection services)
        {
            services.AddIdentity<User, Role>(
                options =>
                    {
                        options.User.RequireUniqueEmail = false;
                    });
            services.AddScoped<DbContext, PolluxDbContext>();
            services.AddScoped<IRoleStore<Role>, RoleStore<Role>>();
            services.AddScoped<IUserStore<User>, UserStore<User>>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ITokenIdentityService, TokenIdentityService>();
            services.AddTransient<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<ClaimsPrincipal, ClaimsPrincipal>();
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<ApplicationLogger>>();
            services.AddSingleton(typeof(ILogger), logger);
        }

        /// <summary>
        /// Adds the identity server services.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddDIIdentityServerServices(this IServiceCollection services)
        {
            services.AddScoped<IResourceStore, ResourceStore>();
            services.AddTransient<IClientStore, ClientStore>();
            services.AddTransient<ICorsPolicyService, CorsPolicyService>();
            services.AddTransient<IProfileService, ProfileService>();
        }

        /// <summary>
        /// Adds the client access token management.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddDIClientAccessTokenManagement(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
            services.AddTransient<ITokenIdentityService, TokenIdentityService>();
            services.AddHttpClient(AccessTokenManagementConstants.BackChannelHttpClientName);
            services.AddHttpContextAccessor();
            services.AddAuthentication();
        }

        /// <summary>
        /// Adds the di miscellaneous.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddDIMiscellaneous(this IServiceCollection services)
        {
            services.AddSingleton<HttpClient, HttpClient>();
            services.AddScoped<AuthEventHandler>();
            services.AddTransient<ISendEmail, SendEmail>();
            services.AddTransient<ISigningCredentialStore, SigningCredentialStore>();
            services.AddTransient<IValidationKeysStore, ValidationKeysStore>();

        }
    }
}
