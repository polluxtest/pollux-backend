namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net.Http;
    using global::IdentityModel.AspNetCore.AccessTokenManagement;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Extension methods for IServiceCollection to register the token management services
    /// </summary>
    public static class TokenManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the access token management.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The options.</param>
        /// <returns>TokenManagementBuilder.</returns>
        public static TokenManagementBuilder AddAccessTokenManagement(
            this IServiceCollection services,
            Action<AccessTokenManagementOptions> options = null)
        {
            if (options != null)
            {
                services.Configure(options);
            }

            services.AddUserAccessTokenManagement();
            services.AddClientAccessTokenManagement();

            return new TokenManagementBuilder(services);
        }


        /// <summary>
        /// Adds the client access token management.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The options.</param>
        /// <returns>TokenManagementBuilder.</returns>
        public static TokenManagementBuilder AddClientAccessTokenManagement(
            this IServiceCollection services,
            Action<AccessTokenManagementOptions> options = null)
        {
            if (options != null)
            {
                services.Configure(options);
            }

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.TryAddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
            services.TryAddTransient<ITokenClientConfigurationService, DefaultTokenClientConfigurationService>();
            services.TryAddTransient<ITokenEndpointService, TokenEndpointService>();
            services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName);

            return new TokenManagementBuilder(services);
        }

        /// <summary>
        /// Adds the services required for user access token management
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static TokenManagementBuilder AddUserAccessTokenManagement(this IServiceCollection services,
            Action<AccessTokenManagementOptions> options = null)
        {
            if (options != null)
            {
                services.Configure(options);
            }

            services.AddHttpContextAccessor();
            services.AddAuthentication();
            services.TryAddTransient<UserAccessTokenHandler>();
            services.TryAddTransient<ITokenClientConfigurationService, DefaultTokenClientConfigurationService>();
            services.TryAddTransient<ITokenEndpointService, TokenEndpointService>();

            services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName);

            return new TokenManagementBuilder(services);
        }

        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the current user access token
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name">The name of the client.</param>
        /// <param name="parameters"></param>
        /// <param name="configureClient">Additional configuration.</param>
        /// <returns></returns>


        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the a client access token
        /// </summary>
        /// <param name="services"></param>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="tokenClientName">The name of the token client.</param>
        /// <param name="configureClient">Additional configuration.</param>
        /// <returns></returns>

    }
}