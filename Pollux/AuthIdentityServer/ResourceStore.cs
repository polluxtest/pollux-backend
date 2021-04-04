namespace Pollux.API.AuthIdentityServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdentityServer4;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;

    public class ResourceStore : IResourceStore
    {
        /// <summary>
        /// Gets identity resources by scope name.
        /// </summary>
        /// <param name="scopeNames">The scopes of the api</param>
        /// <returns>Empty List Not Implementation.</returns>
        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return new List<IdentityResource>();
        }

        /// <summary>
        /// Finds the API scopes by name asynchronous.
        /// </summary>
        /// <param name="scopeNames">The scope names.</param>
        /// <returns>The Scopes of the api.</returns>
        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            return new List<ApiScope>()
                {
                    new ApiScope() { Name = "api" },
                    new ApiScope() { Name = "api/pollux" },
                    new ApiScope() { Name = IdentityServerConstants.StandardScopes.OfflineAccess },
                };
        }

        /// <summary>
        /// Finds the API resources by scope name asynchronous.
        /// </summary>
        /// <param name="scopeNames">The scope names.</param>
        /// <returns>Api Resources.</returns>
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return new List<ApiResource>();
        }

        /// <summary>
        /// Finds the API resources by name asynchronous.
        /// </summary>
        /// <param name="apiResourceNames">The API resource names.</param>
        /// <returns>Api Resources.</returns>
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            return new List<ApiResource>();
        }

        /// <summary>
        /// Gets all resources asynchronous.
        /// </summary>
        /// <returns>Resources.</returns>
        public async Task<Resources> GetAllResourcesAsync()
        {
            return new Resources();
        }
    }
}
