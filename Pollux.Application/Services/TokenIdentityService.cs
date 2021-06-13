namespace Pollux.Application.Services
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using IdentityServer4.Models;
    using Microsoft.Extensions.Logging;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Application.Models.Settings;
    using Pollux.Common.Constants.Strings;

    public interface ITokenIdentityService
    {
        /// <summary>
        /// Refreshes the user access token asynchronous.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>TokenResponse.</returns>
        Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken);

        /// <summary>
        /// Requests the client access token.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="loginModel">The login model.</param>
        /// <returns>TokenResponse.</returns>
        Task<TokenResponse> RequestClientAccessToken(string clientName, LogInModel loginModel);

        /// <summary>
        /// Revokes the refresh token asynchronous.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>TokenRevocationResponse.</returns>
        Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implements token endpoint operations using IdentityModel
    /// </summary>
    public class TokenIdentityService : ITokenIdentityService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<TokenIdentityService> logger;
        private readonly IdentityServerSettings identityServerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenIdentityService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="identityServerSettings">identity Server Settings.</param>
        public TokenIdentityService(
            IHttpClientFactory httpClientFactory,
            ILogger<TokenIdentityService> logger,
            IdentityServerSettings identityServerSettings)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.identityServerSettings = identityServerSettings;
        }


        /// <summary>
        /// Request to Identity Server an access token and refresh token upon log in.
        /// </summary>
        /// <param name="clientName">The client name of IS.</param>
        /// <param name="loginModel">user parameters.</param>
        /// <returns>TokenResponse.</returns>
        public async Task<TokenResponse> RequestClientAccessToken(string clientName, LogInModel loginModel)
        {
            this.logger.LogDebug("Requesting client access token for client: {client}", clientName);

            var clientCredentials = new IdentityModel.Client.PasswordTokenRequest()
            {
                Method = HttpMethod.Post,
                Address = this.identityServerSettings.HostUrl,
                ClientId = IdentityServerConstants.ClientName,
                ClientSecret = IdentityServerConstants.ClientSecret,
                GrantType = GrantTypes.ResourceOwnerPassword.First(),
                Scope = $"{IdentityServerConstants.Scope} {IdentityServerConstants.RequestRefreshToken}",
                UserName = loginModel.Email,
                Password = loginModel.Password,
            };

            var httpClient = this.httpClientFactory.CreateClient(AccessTokenManagementConstants.BackChannelHttpClientName);
            return await httpClient.RequestPasswordTokenAsync(clientCredentials);
        }

        /// <summary>
        /// Refresh User Access Token Async.
        /// </summary>
        /// <param name="refreshToken">refreshToken.</param>
        /// <returns>TokenResponse.</returns>
        public async Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken)
        {
            this.logger.LogDebug("Refreshing refresh token: {token}", refreshToken);

            var refreshTokenRequest = new RefreshTokenRequest()
            {
                Method = HttpMethod.Post,
                Address = this.identityServerSettings.HostUrl,
                ClientId = IdentityServerConstants.ClientNameRefreshToken,
                ClientSecret = IdentityServerConstants.ClientSecret,
                GrantType = IdentityServerConstants.GrantAccessRefreshToken,
                RefreshToken = refreshToken,
            };

            var httpClient = this.httpClientFactory.CreateClient(AccessTokenManagementConstants.BackChannelHttpClientName);
            return await httpClient.RequestRefreshTokenAsync(refreshTokenRequest);
        }

        /// <summary>
        /// Revokes Refresh Token by not allowing the user access resources in case the refresh token has been compromised or you
        /// enchanged already a token for a refresh token.
        /// </summary>
        /// <param name="refreshToken">refreshToken</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>TokenRevocationResponse.</returns>
        public async Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            this.logger.LogDebug("Revoking refresh token: {token}", refreshToken);

            var tokenRevocationRequest = new TokenRevocationRequest()
            {
                Method = HttpMethod.Post,
                Address = this.identityServerSettings.HostUrl,
                ClientId = IdentityServerConstants.ClientName,
                ClientSecret = IdentityServerConstants.ClientSecret,
            };

            var httpClient = this.httpClientFactory.CreateClient(AccessTokenManagementConstants.BackChannelHttpClientName);
            return await httpClient.RevokeTokenAsync(tokenRevocationRequest);
        }
    }
}