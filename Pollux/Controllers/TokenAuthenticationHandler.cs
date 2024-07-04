using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Pollux.API;

namespace Pollux.API.Auth
{
    /// <summary>
    /// TokenAuthenticationOptions.
    /// </summary>
    /// <seealso cref="AuthenticationSchemeOptions" />
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    { }

    /// <summary>
    /// TokenAuthenticationHandler.
    /// </summary>
    /// <seealso cref="TokenAuthenticationOptions" />
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly ILogger<ApplicationLogger> logger;

        public TokenAuthenticationHandler(
            IOptionsMonitor<TokenAuthenticationOptions> options,
            ILogger<ApplicationLogger> logger,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
            : base(options, loggerFactory, encoder, clock)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
            this.logger = logger;
        }

        /// <summary>
        /// Handles the authenticate asynchronous.
        /// </summary>
        /// <returns>AuthenticateResult.</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                Request.Headers.TryGetValue("Authorization", out var token);
                logger.LogInformation($"token {token}");
                if (string.IsNullOrEmpty(token))
                {
                    logger.LogInformation($"Initial Authorization token empty {token}");
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                var claims = new[] { new Claim("token", token) };
                var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

                if (!string.IsNullOrEmpty(token))
                {
                    logger.LogInformation("token is not null and token decoded is the same ok.");
                    if (token.ToString().Contains("Bearer", StringComparison.OrdinalIgnoreCase))
                    {
                        token = token.ToString().Remove(0, 7);
                        bool isValid = ValidateToken(token);
                        if (isValid)
                        {
                            logger.LogInformation("Token is valid");
                            return Task.FromResult(AuthenticateResult.Success(ticket));
                        }
                        else
                        {
                            logger.LogInformation("Token is invalid");
                            return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
                        }
                    }
                }

                logger.LogInformation("Authorization failed 401");
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
            }
            catch (Exception ex)
            {
                logger.LogInformation("Authorization exception");
                logger.LogInformation(ex.Message);
                logger.LogInformation(ex.StackTrace);
                logger.LogInformation(ex?.InnerException?.Message);
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
            }
        }

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token.</param>
        private bool ValidateToken(string token)
        {
            try
            {
                var tokenIssuer = configuration.GetSection("AppSettings")["TokenIssuer"];
                logger.LogInformation($"token issuer app settings {tokenIssuer}");
                var signingKeyId = configuration.GetSection("AppSettings")["SigningKeyId"];
                logger.LogInformation($"signing key app settings {signingKeyId}");

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.ReadJwtToken(token);
                logger.LogInformation($"token decoded jwt issuer {securityToken.Issuer}");
                var securityTokenSigningkeyId = securityToken.Header["kid"];
                logger.LogInformation($"signing key request header {securityTokenSigningkeyId}");

                if (securityToken.Issuer.Equals(tokenIssuer))
                {
                    logger.LogInformation("Token issuer matched");
                    if (signingKeyId.Equals(securityTokenSigningkeyId))
                    {
                        logger.LogInformation("Signing key matched");
                        var claims = securityToken.Claims.ToList();
                        logger.LogInformation($"claims  {claims}");
                        logger.LogInformation("token issuer and token decoded is the same ok.");
                        var expiration = claims[1].Value;

                        logger.LogInformation($"expiration  {expiration}");

                        var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiration));
                        if (DateTime.UtcNow > expirationDateTime)
                        {
                            logger.LogInformation($"expired token");
                            return false;
                        }
                    }
                    else
                    {
                        logger.LogInformation("signing key NOT MATCHED");
                    }
                }
                else
                {
                    logger.LogInformation("token issuer NOT MATCHED");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"error exception  {ex.Message}");
                logger.LogError($"error exception  {ex.StackTrace}");
                logger.LogError($"error exception  {ex.InnerException}");

                return false;
            }
        }
    }
}



