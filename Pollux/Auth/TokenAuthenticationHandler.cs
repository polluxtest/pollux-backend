namespace Pollux.API.Auth
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Pollux.Application.Services;
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Common.Constants;
    using Pollux.Common.Constants.Strings.Api;
    using Pollux.Persistence.Services.Cache;

    /// <summary>
    /// TokenAuthenticationOptions.
    /// </summary>
    /// <seealso cref="AuthenticationSchemeOptions" />
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    /// <summary>
    /// TokenAuthenticationHandler.
    /// </summary>
    /// <seealso cref="TokenAuthenticationOptions" />
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<ApplicationLogger> logger;

        private readonly IRedisCacheService redisCacheService;

        private readonly ITokenIdentityService tokenIdentityService;

        private readonly IList<string> skipApiRoutes;

        public TokenAuthenticationHandler(
            IRedisCacheService redisCacheService,
            ITokenIdentityService tokenIdentityService,
            IOptionsMonitor<TokenAuthenticationOptions> options,
            ILogger<ApplicationLogger> logger,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration)
            : base(options, loggerFactory, encoder, clock)
        {
            this.tokenIdentityService = tokenIdentityService;
            this.logger = logger;
            this.configuration = configuration;
            this.redisCacheService = redisCacheService;
            this.skipApiRoutes = new List<string>() { ApiConstants.Preferences };
        }

        /// <summary>
        /// Handles the authenticate asynchronous.
        /// </summary>
        /// <returns>AuthenticateResult.</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                this.Request.Headers.TryGetValue("Authorization", out var token);
                if (string.IsNullOrEmpty(token))
                {
                    return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
                }

                var claims = new[] { new Claim("token", token) };
                var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);

                token = token.ToString().Remove(0, 7);
                if (!string.IsNullOrEmpty(token) && this.ValidateToken(token))
                {
                    this.logger.LogInformation("Token is valid");
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                else
                {
                    this.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogInformation("Authorization exception");
                this.logger.LogInformation(ex.Message);
                this.logger.LogInformation(ex.StackTrace);
                this.logger.LogInformation(ex?.InnerException?.Message);
                this.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
            }
        }

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>treu/false</returns>
        private bool ValidateToken(string token)
        {
            try
            {
                var tokenIssuer = this.configuration.GetSection("AppSettings")["TokenIssuer"];
                var signingKeyId = this.configuration.GetSection("AppSettings")["SigningKeyId"];

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.ReadJwtToken(token);
                var securityTokenSigningkeyId = securityToken.Header["kid"];

                if (securityToken.Issuer.Equals(tokenIssuer))
                {
                    this.logger.LogInformation("Token issuer matched");
                    if (signingKeyId.Equals(securityTokenSigningkeyId))
                    {
                        var claims = securityToken.Claims.ToList();
                        var expiration = claims[1].Value;

                        var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiration)).UtcDateTime;
                        if (DateTime.UtcNow >= expirationDateTime)
                        {
                            var currentEndpointPath = this.Request.Path.Value;
                            if (this.skipApiRoutes.Contains(currentEndpointPath))
                            {
                                return false;
                            }

                            this.logger.LogInformation($"expired token");
                            var userId = claims.First(p => p.Type.Equals("sub")).Value;
                            this.RefreshAccessTokenAsync(userId);
                            return true;
                        }
                    }
                    else
                    {
                        this.logger.LogInformation("signing key NOT MATCHED");
                    }
                }
                else
                {
                    this.logger.LogInformation(tokenIssuer);
                    this.logger.LogInformation(securityToken.Issuer);
                    this.logger.LogInformation("token issuer NOT MATCHED");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"error exception  {ex.Message}");
                this.logger.LogError($"error exception  {ex.StackTrace}");
                this.logger.LogError($"error exception  {ex.InnerException}");

                return false;
            }
        }

        /// <summary>
        /// Sets the new access token.
        /// </summary>
        /// <param name="email">The userId.</param>
        /// <param name="tokenModel">The token model.</param>
        private async void SetNewAccessToken(string email, TokenModel tokenModel)
        {
            await this.redisCacheService.DeleteKeyAsync(email);
            var success = await this.redisCacheService.SetObjectAsync(email, tokenModel);
        }

        /// <summary>
        /// Gets the authentication from redis.
        /// </summary>
        /// <param name="email">The userId.</param>
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
        /// Refersh Access Tokem
        /// </summary>
        /// <param name="userId">userId</param>
        private void RefreshAccessTokenAsync(string userId)
        {
            var tokenModel = this.GetAuthFromRedis(userId).Result;

            if (tokenModel == null || DateTime.UtcNow >= tokenModel.RefreshTokenExpirationDate)
            {
                throw new Exception("Token model does not exist in redis or not able to refresh access token");
            }

            if (DateTime.UtcNow >= tokenModel.AccessTokenExpirationDate)
            {
                var tokenResponse = this.tokenIdentityService.RefreshUserAccessTokenAsync(tokenModel.RefreshToken)
                    .Result;
                tokenModel.AccessToken = tokenResponse.AccessToken;
                tokenModel.AccessTokenExpirationDate =
                    DateTime.UtcNow.AddSeconds(ExpirationConstants.AccessTokenExpiratioSeconds);
                this.SetNewAccessToken(userId, tokenModel);
                this.Response.Headers["set-cookie"] = tokenModel.AccessToken;
            }
        }
    }
}
