using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pollux.API
{
    using IdentityServer4;
    using IdentityServer4.Models;

    public class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                };


        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
                {
                    new ApiScope("api", "My API"),
                    new ApiScope("api/pollux", "My API"),
                    new ApiScope("offline_access", "offline_access")
                };

        public static IEnumerable<Client> Clients =>
            new List<Client>
                {
                    // machine to machine client
                    new Client
                        {
                            ClientId = "client",
                            ClientSecrets = { new Secret("secret".Sha256()) },
                            AllowOfflineAccess = true,
                            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                            RefreshTokenExpiration = TokenExpiration.Absolute,
                            AbsoluteRefreshTokenLifetime = 60,
                            SlidingRefreshTokenLifetime = 60,
                            // scopes that client has access to
                            AllowedScopes = new List<string>(){ "api","api/pollux","offline_access"}
                        },
                
                    // interactive ASP.NET Core MVC client
                    new Client
                        {
                            ClientId = "client",
                            ClientSecrets = { new Secret("secret".Sha256()) },

                            AllowedGrantTypes =  new List<string>() { "refresh_token" },

                            RefreshTokenExpiration = TokenExpiration.Absolute,
                            AbsoluteRefreshTokenLifetime = 60,
                            SlidingRefreshTokenLifetime = 60,

                            AllowedScopes = new List<string>(){ "api","api/pollux"}
                        }
                };


    }
}
