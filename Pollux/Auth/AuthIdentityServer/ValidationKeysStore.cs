namespace Pollux.API.Auth.AuthIdentityServer
{
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.IdentityModel.Tokens;

    public class ValidationKeysStore : IValidationKeysStore
    {
        /// <summary>
        /// Gets all validation keys.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var rsa = RSA.Create();
            var securityKey = new RsaSecurityKey(rsa);

            var securityKeyInfo = new SecurityKeyInfo
            {
                Key = securityKey,
            };

            var keys = new List<SecurityKeyInfo>() { securityKeyInfo };

            return Task.FromResult((IEnumerable<SecurityKeyInfo>)keys);
        }
    }
}