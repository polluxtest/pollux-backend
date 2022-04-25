namespace Pollux.API.AuthIdentityServer
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

        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var rsa = RSA.Create();
            var securityKey = new RsaSecurityKey(rsa);
            securityKey.KeyId = this.signingKeyId;
            var signInCredentials = new SigningCredentials(securityKey, "RS256");

            return Task.FromResult(signInCredentials);
        }
    }
}
