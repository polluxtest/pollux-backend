namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using global::IdentityModel.AspNetCore.AccessTokenManagement;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Pollux.Common.Constants;

    /// <summary>
    /// Extension methods for IServiceCollection to register the token management services
    /// </summary>
    public static class TokenServiceDIExtensions
    {
        /// <summary>
        /// Adds the access token management.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The options.</param>
        /// <returns>TokenManagementBuilder.</returns>


        /// <summary>
        /// Adds the client access token management.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The options.</param>
        /// <returns>TokenManagementBuilder.</returns>
        public static void AddClientAccessTokenManagement(
            this IServiceCollection services)
        {

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.TryAddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
            services.TryAddTransient<ITokenIdentityService, TokenIdentityService>();
            services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
            services.AddHttpContextAccessor();
            services.AddAuthentication();
            services.TryAddTransient<ITokenIdentityService, TokenIdentityService>();

            services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
        }


    }
}