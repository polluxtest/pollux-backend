using Pollux.Application.Services;

namespace Pollux.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.Application;
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Common.Exceptions;
    using Pollux.Persistence.Services.Cache;

    public class AuthEventHandler
    {
        private readonly List<string> anonymousRoutes;
        private readonly IRedisCacheService redisCacheService;
        private readonly ITokenIdentityService tokenIdentityService;
        private readonly IUsersService userService;
        private readonly IAuthService authService;

        public AuthEventHandler(
            ITokenIdentityService tokenIdentityService,
            IRedisCacheService redisCacheService,
            IUsersService userService,
            IAuthService authService)
        {
            this.anonymousRoutes = new List<string>() { "SignUp", "LogIn", "ResetPassword", "LogOut", "Exist" };
            this.tokenIdentityService = tokenIdentityService;
            this.redisCacheService = redisCacheService;
            this.userService = userService;
            this.authService = authService;
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

            if (!context.Principal.Identity.IsAuthenticated ||
                emailClaim == null ||
                !authValues.Any())
            {
                this.RevokeAuth(context.HttpContext, emailClaim?.Value);
            }

            var tokenModel = await this.GetAuthFromRedis(emailClaim?.Value);
            var accessToken = authValues.First();

            if (this.IsRefreshTokenExpired(tokenModel, accessToken))
            {
                this.RevokeAuth(context.HttpContext, emailClaim?.Value);
            }

            return await this.IsAccessTokenExpired(emailClaim?.Value, tokenModel);
        }

        /// <summary>
        /// Skips the anonymous routes.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>True/False.</returns>
        private bool SkipAnonymousRoutes(string route)
        {
            foreach (var anonymousRoute in this.anonymousRoutes)
            {
                if (route.EndsWith(anonymousRoute))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the authentication from redis.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>The Auth from Redis.</returns>
        private async Task<TokenModel> GetAuthFromRedis(string email)
        {
            var token = await this.redisCacheService.GetObjectAsync<TokenModel>(email);

            return token;
        }

        /// <summary>
        /// Determines whether [is refresh token expired] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>
        ///   <c>true</c> if [is refresh token expired] [the specified token]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRefreshTokenExpired(TokenModel token, string accessToken)
        {
            return !token.AccessToken.Equals(accessToken) ||
                    string.IsNullOrEmpty(token.AccessToken) ||
                    DateTime.UtcNow > token.RefreshTokenExpirationDate;
        }

        /// <summary>
        /// Determines whether [is access token expired] [the specified email].
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="tokenModel">The token model.</param>
        /// <returns>Token response , access token.</returns>
        private async Task<TokenResponse> IsAccessTokenExpired(string email, TokenModel tokenModel)
        {
            if (DateTime.UtcNow > tokenModel.AccessTokenExpirationDate)
            {
                var newAccessToken = await this.tokenIdentityService.RefreshUserAccessTokenAsync(tokenModel.RefreshToken);
                tokenModel.AccessToken = newAccessToken.AccessToken;
                tokenModel.AccessTokenExpirationDate = DateTime.UtcNow.AddMinutes(10); // todo change expiration
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
            var success = await this.redisCacheService.SetObjectAsync<TokenModel>(email, tokenModel, TimeSpan.FromDays(7));
        }

        /// <summary>
        /// Revokes the authentication must login again.
        /// </summary>
        /// <exception cref="NotAuthenticatedException">not authenticated</exception>
        private async void RevokeAuth(HttpContext httpContext, string username)
        {
            await httpContext.SignOutAsync();
            await this.userService.LogOutAsync();
            await this.authService.RemoveAuth(username);
            throw new NotAuthenticatedException("not authenticated");
        }
    }
}
