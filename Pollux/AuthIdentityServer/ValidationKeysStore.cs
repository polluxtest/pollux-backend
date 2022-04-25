namespace Pollux.API.AuthIdentityServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.IdentityModel.Tokens;

    public class ValidationKeysStore : IValidationKeysStore
    {
        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var rsa = RSA.Create();
            var securityKey = new RsaSecurityKey(rsa);

            var securityKeyInfo = new SecurityKeyInfo();
            securityKeyInfo.Key = securityKey;

            var keys = new List<SecurityKeyInfo>() { securityKeyInfo };

            return Task.FromResult((IEnumerable<SecurityKeyInfo>)keys);
        }
    }
}