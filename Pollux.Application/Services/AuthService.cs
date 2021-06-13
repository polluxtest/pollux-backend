using Pollux.Application.Services;

namespace Pollux.Application
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings;
    using Pollux.Common.Exceptions;
    using Pollux.Persistence.Services.Cache;

    public interface IAuthService
    {
        Task<TokenResponse> SetAuth(LogInModel loginModel);

        Task<bool> RemoveAuth(string key);

        Task<TokenResponse> CheckAuth(string email = null);
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

        private readonly ClaimsPrincipal identity;

        public AuthService(
            ITokenIdentityService tokenService,
            IRedisCacheService redisCacheService,
            ClaimsPrincipal identity)
        {
            this.tokenService = tokenService;
            this.redisCacheService = redisCacheService;
            this.identity = identity;
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
            var accessTokenExpirationDate = DateTime.UtcNow.AddMinutes(10); // todo change this values
            var refreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(20);

            var tokenCache = new TokenModel()
            {
                AccessToken = $"{OAuthConstants.JWTAuthScheme} {tokenResponse.AccessToken}",
                RefreshToken = tokenResponse.RefreshToken,
                AccessTokenExpirationDate = accessTokenExpirationDate,
                RefreshTokenExpirationDate = refreshTokenExpirationDate,
                Password = loginModel.Password,
            };

            await this.redisCacheService.SetObjectAsync<TokenModel>(loginModel.Email, tokenCache, TimeSpan.FromHours(1)); // this must match expiration of token ??

            return tokenResponse;
        }

        public async Task<TokenResponse> CheckAuth(string email = null)
        {
            var userLoggedEmail = this.identity.Claims.Single(p => p.Type == ClaimTypes.Email).Value;

            var authRedis = await this.redisCacheService.GetObjectAsync<TokenModel>(userLoggedEmail);
            if (DateTime.Now > authRedis.RefreshTokenExpirationDate)
            {
                throw new NotAuthenticatedException("Not Authenticated , must log in again");
            }

            if (DateTime.Now > authRedis.AccessTokenExpirationDate)
            {
                var loginModel = new LogInModel() { Email = userLoggedEmail, Password = authRedis.Password };
                var response = await this.tokenService.RequestClientAccessToken(authRedis.RefreshToken, loginModel);

                return response;
            }

            return null;
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
