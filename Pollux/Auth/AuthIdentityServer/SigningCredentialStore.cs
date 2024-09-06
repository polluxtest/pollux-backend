namespace Pollux.API.Auth.AuthIdentityServer
{
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdentityServer4.Stores;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;

    public class SigningCredentialStore : ISigningCredentialStore
    {
        private readonly string signingKeyId;

        public SigningCredentialStore(IConfiguration configuration)
            : base()
        {
            this.signingKeyId = configuration.GetSection("AppSettings")["SigningKeyId"];
        }

        /// <summary>
        /// Gets the signing credentials.
        /// </summary>
        /// <returns>SigningCredentials</returns>
        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var rsa = RSA.Create();
            var securityKey = new RsaSecurityKey(rsa)
            {
                KeyId = this.signingKeyId,
            };
            var signInCredentials = new SigningCredentials(securityKey, "RS256");

            return Task.FromResult(signInCredentials);
        }
    }
}
