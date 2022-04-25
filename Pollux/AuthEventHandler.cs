namespace Pollux.API
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.Application;
    using Pollux.Application.Services;
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Common.Constants;
    using Pollux.Common.Constants.Strings;
    using Pollux.Common.Exceptions;
    using Pollux.Persistence.Services.Cache;

    public class AuthEventHandler
    {
        private readonly List<string> anonymousRoutes;
        private readonly IRedisCacheService redisCacheService;
        private readonly ITokenIdentityService tokenIdentityService;
        private readonly IUsersService userService;
        private readonly IAuthService authService;
        private readonly IConfiguration configuration;

        public AuthEventHandler(
            ITokenIdentityService tokenIdentityService,
            IRedisCacheService redisCacheService,
            IUsersService userService,
            IAuthService authService,
            IConfiguration configuration)
        {
            this.anonymousRoutes = new List<string>() { "SignUp", "LogIn", "ResetPassword", "LogOut", "Exist" };
            this.tokenIdentityService = tokenIdentityService;
            this.redisCacheService = redisCacheService;
            this.userService = userService;
            this.authService = authService;
            this.configuration = configuration;
        }

        /// <summary>
        /// Handles the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>New Access token if need it or Revoke Auth.</returns>
        public async Task<TokenResponse> Handle(CookieValidatePrincipalContext context)
        {
            if (this.SkipAnonymousRoutes(context.Request.Path.Value))
            {
                return null;
            }

            var emailClaim = context.Principal.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email);
            context.Request.Headers.TryGetValue("Authorization", out var authValues);
            var accessToken = authValues.First();

            if (!context.Principal.Identity.IsAuthenticated ||
                emailClaim == null ||
                !authValues.Any())
            {
                this.RevokeAuth(context.HttpContext, emailClaim?.Value);
            }

            var tokenModel = await this.GetAuthFromRedis(emailClaim?.Value);
            if (tokenModel == null ||
                this.IsRefreshTokenExpired(tokenModel) ||
                !this.ValidateIssuer(tokenModel.AccessToken) ||
                string.IsNullOrEmpty(tokenModel.AccessToken))
            {
                this.RevokeAuth(context.HttpContext, emailClaim?.Value);
            }

            return await this.IsAccessTokenExpired(emailClaim?.Value, tokenModel, context.HttpContext);
        }

        /// <summary>
        /// Skips the anonymous routes.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>True/False.</returns>
        private bool SkipAnonymousRoutes(string route)
        {
            // todo this could have a faster solution.
            var routeSegments = route.Split("/");
            return this.anonymousRoutes.Any(p => p == routeSegments[routeSegments.Length - 1]);
        }

        /// <summary>
        /// Gets the authentication from redis.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>The Auth from Redis.</returns>
        private async Task<TokenModel> GetAuthFromRedis(string email)
        {
            var exists = await this.redisCacheService.KeyExistsAsync(email);
            if (!exists)
            {
                return null;
            }

            var token = await this.redisCacheService.GetObjectAsync<TokenModel>(email);

            return token;
        }

        /// <summary>
        /// Determines whether [is refresh token expired] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>        /// <returns>
        ///   <c>true</c> if [is refresh token expired] [the specified token]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRefreshTokenExpired(TokenModel token)
        {
            return DateTime.UtcNow > token.RefreshTokenExpirationDate;
        }

        /// <summary>
        /// Determines whether [is access token expired] [the specified email].
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="tokenModel">The token model.</param>
        /// <returns>Token response , access token.</returns>
        private async Task<TokenResponse> IsAccessTokenExpired(string email, TokenModel tokenModel, HttpContext httpContext)
        {

            if (tokenModel == null)
            {
                this.WriteSessionExpired(httpContext);
                return null;
            }

            if (DateTime.UtcNow > tokenModel.AccessTokenExpirationDate)
            {
                var newAccessToken = await this.tokenIdentityService.RefreshUserAccessTokenAsync(tokenModel.RefreshToken);
                tokenModel.AccessToken = $"{OAuthConstants.JWTAuthScheme} {newAccessToken.AccessToken}";
                tokenModel.AccessTokenExpirationDate = DateTime.UtcNow.AddSeconds(ExpirationConstants.AccessTokenExpirationSeconds);
                tokenModel.RefreshToken = newAccessToken.RefreshToken;
                this.SetNewAccessToken(email, tokenModel);

                return newAccessToken;
            }

            return null;
        }

        /// <summary>
        /// Sets the new access token.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="tokenModel">The token model.</param>
        private async void SetNewAccessToken(string email, TokenModel tokenModel)
        {
            var success = await this.redisCacheService.SetObjectAsync<TokenModel>(email, tokenModel);
        }

        /// <summary>
        /// Revokes the authentication must login again.
        /// </summary>
        private async void RevokeAuth(HttpContext httpContext, string username)
        {
            await httpContext.SignOutAsync();
            await this.userService.LogOutAsync();
            await this.authService.RemoveAuth(username);

            this.WriteSessionExpired(httpContext);
        }

        /// <summary>
        /// Validates the issuer.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>true/false.</returns>
        private bool ValidateIssuer(string token)
        {
            try
            {
                token = token.ToString().Remove(0, 7);
                var tokenIssuer = this.configuration.GetSection("AppSettings")["TokenIssuer"];
                var signingKeyId = this.configuration.GetSection("AppSettings")["SigningKeyId"];
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.ReadJwtToken(token);
                var securityTokenSigningKeyId = securityToken.Header["kid"];
                return securityToken.Issuer.Equals(tokenIssuer) && signingKeyId.Equals(securityTokenSigningKeyId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Writes the session expired.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        private void WriteSessionExpired(HttpContext httpContext)
        {
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = 440;
            }
            else
            {
                httpContext.Response.WriteAsync(MessagesConstants.NotAuthenticated);
            }
        }
    }
}
