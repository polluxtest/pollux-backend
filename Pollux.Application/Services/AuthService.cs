namespace Pollux.Application.Services
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Microsoft.Extensions.Logging;
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants;
    using Pollux.Common.Constants.Strings;
    using Pollux.Persistence.Services.Cache;

    public interface IAuthService
    {
        Task<TokenResponse> SetAuth(LogInModel loginModel);

        Task<bool> RemoveAuth(string key);
    }

    public class AuthService : IAuthService
    {
        /// <summary>
        /// The token service.
        /// </summary>
        private readonly ITokenIdentityService tokenService;

        /// <summary>
        /// The redis cache service.
        /// </summary>
        private readonly IRedisCacheService redisCacheService;

        /// <summary>
        /// The identity.
        /// </summary>
        private readonly ClaimsPrincipal identity;
        private readonly ILogger logger;

        public AuthService(
            ILogger logger,
            ITokenIdentityService tokenService,
            IRedisCacheService redisCacheService,
            ClaimsPrincipal identity)
        {
            this.tokenService = tokenService;
            this.redisCacheService = redisCacheService;
            this.identity = identity;
            this.logger = logger;
        }


        /// <summary>
        /// Sets the authentication requesting a token and saving it in redis cache.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns>TokenResponse.</returns>
        /// <exception cref="InvalidOperationException">Could Store key in redis data base</exception>
        public async Task<TokenResponse> SetAuth(LogInModel loginModel)
        {
            var tokenResponse = await this.tokenService.RequestClientAccessToken(IdentityServerConstants.ClientName, loginModel);

            if (tokenResponse == null)
            {
                this.logger.LogInformation($"response from token request  is null");

            }
            this.logger.LogInformation($"response from token request {tokenResponse.HttpStatusCode}");
            this.logger.LogInformation($"response from token request is error {tokenResponse.IsError}");
            this.logger.LogInformation($"response from token request is reason {tokenResponse?.HttpErrorReason}");
            this.logger.LogInformation($"response from token request is error {tokenResponse.Exception.Message}");

            var accessTokenExpirationDate = DateTime.UtcNow.AddSeconds(ExpirationConstants.AccessTokenExpirationSeconds);
            var refreshTokenExpirationDate = DateTime.UtcNow.AddSeconds(ExpirationConstants.RefreshTokenExpirationSeconds);

            var tokenCache = new TokenModel()
            {
                AccessToken = $"{OAuthConstants.JWTAuthScheme} {tokenResponse.AccessToken}",
                RefreshToken = tokenResponse.RefreshToken,
                AccessTokenExpirationDate = accessTokenExpirationDate,
                RefreshTokenExpirationDate = refreshTokenExpirationDate,
                Password = loginModel.Password,
            };

            await this.redisCacheService.SetObjectAsync<TokenModel>(loginModel.Email, tokenCache);

            return tokenResponse;
        }

        /// <summary>
        /// Removes the authentication from cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if success.</returns>
        public Task<bool> RemoveAuth(string key)
        {
            return this.redisCacheService.DeleteKeyAsync(key);
        }
    }
}
