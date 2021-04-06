namespace Pollux.API.AuthIdentityServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;

    public class ClientStore : IClientStore
    {
        /// <summary>
        /// Finds a identty server client configuration by id.
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            if (clientId == "client")

                return new Client()
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = new List<string>() { "api", "api/pollux", "offline_access" },
                    AccessTokenLifetime = 5,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    IdentityTokenLifetime = 50,
                    AbsoluteRefreshTokenLifetime = 50,
                    SlidingRefreshTokenLifetime = 50,
                };

            if (clientId == "x")
                return new Client()
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = new List<string>() { "refresh_token" },
                    AllowedScopes = new List<string>() { "api", "api/pollux" },
                    AccessTokenLifetime = 5,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    IdentityTokenLifetime = 50,
                    AbsoluteRefreshTokenLifetime = 50,
                    SlidingRefreshTokenLifetime = 50,
                };

            return null;
        }
    }
}
