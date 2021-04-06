namespace Pollux.API.AuthIdentityServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Pollux.Common.Constants.Strings;

    public class ClientStore : IClientStore
    {
        /// <summary>
        /// Finds a identty server client configuration by id.
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client.
        /// </returns>
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            if (clientId == IdentityServerConstants.ClientName)
            {
                return new Client()
                {
                    ClientId = IdentityServerConstants.ClientName,
                    ClientSecrets = { new Secret(IdentityServerConstants.ClientSecret.Sha256()) },
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = new List<string>(GrantTypes.ResourceOwnerPassword) { "refresh_token" },
                    AllowedScopes = new List<string>() { "api", "api/pollux", "offline_access" },
                    AccessTokenLifetime = 5,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    IdentityTokenLifetime = 50,
                    AbsoluteRefreshTokenLifetime = 50,
                    SlidingRefreshTokenLifetime = 50,
                };
            }

            if (clientId == IdentityServerConstants.ClientNameRefreshToken)
            {
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
            }

            return null;
        }
    }
}
