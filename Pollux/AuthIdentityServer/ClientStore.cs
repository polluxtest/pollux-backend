namespace Pollux.API.AuthIdentityServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Pollux.Common.Constants;
    using Pollux.Common.Constants.Strings;

    public class ClientStore : IClientStore
    {
        /// <summary>
        /// Finds a client by id.
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client based on the identity server request type.
        /// </returns>
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            if (clientId == IdentityServerConstants.ClientName)
            {
                return Task.FromResult(new Client()
                {
                    ClientId = IdentityServerConstants.ClientName,
                    ClientSecrets = { new Secret(IdentityServerConstants.ClientSecret.Sha256()) },
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = new List<string>(GrantTypes.ResourceOwnerPassword) { "refresh_token" },
                    AllowedScopes = new List<string>() { "api", "api/pollux", "offline_access" },
                    AccessTokenLifetime = ExpirationConstants.AccessTokenExpiration,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    IdentityTokenLifetime = ExpirationConstants.RefreshTokenExpiration,
                    AbsoluteRefreshTokenLifetime = ExpirationConstants.RefreshTokenExpiration,
                    SlidingRefreshTokenLifetime = ExpirationConstants.RefreshTokenExpiration,
                });
            }

            if (clientId == IdentityServerConstants.ClientNameRefreshToken)
            {
                return Task.FromResult(new Client()
                {
                    ClientId = IdentityServerConstants.ClientName,
                    ClientSecrets = { new Secret(IdentityServerConstants.ClientSecret.Sha256()) },
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = new List<string>(GrantTypes.ResourceOwnerPassword),
                    AllowedScopes = new List<string>() { "api", "api/pollux" },
                    AccessTokenLifetime = ExpirationConstants.AccessTokenExpiration,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    IdentityTokenLifetime = ExpirationConstants.RefreshTokenExpiration,
                    AbsoluteRefreshTokenLifetime = ExpirationConstants.RefreshTokenExpiration,
                    SlidingRefreshTokenLifetime = ExpirationConstants.RefreshTokenExpiration,
                });
            }

            return null;
        }
    }
}
