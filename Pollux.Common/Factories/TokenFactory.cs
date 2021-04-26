namespace Pollux.Common.Factories
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;

    /// <summary>
    /// Generate tokens with or without expiration dates and also decodes tokens
    /// </summary>
    public static class TokenFactory
    {
        /// <summary>
        /// Decodes the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The decrypted token as mail.</returns>
        /// <exception cref="ArgumentException">The token is invalid - token</exception>
        public static string DecodeToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken == null)
            {
                throw new ArgumentException("The token is invalid", nameof(token));
            }

            var email = ((List<Claim>)jsonToken.Claims).Find(p => p.Type == ClaimTypes.Email).Value;

            return email;
        }
    }
}
