namespace Pollux.API
{
    using System.Collections.Generic;
    using IdentityServer4.Models;

    public class IdentityServerConfig
    {
        /// <summary>
        /// Gets the identity resources.
        /// </summary>
        /// <value>
        /// The identity resources.
        /// </value>
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                };

        /// <summary>
        /// Gets the API scopes.
        /// </summary>
        /// <value>
        /// The API scopes.
        /// </value>
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
                {
                    new ApiScope("api", "My API"),
                    new ApiScope("api/pollux", "My API"),
                    new ApiScope("offline_access", "offline_access"),
                };
    }
}
