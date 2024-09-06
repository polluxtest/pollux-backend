namespace Pollux.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Linq;


    /// <summary>
    /// Generate tokens with or without expiration dates and also decodes tokens
    /// </summary>
    public static class TokenUtility
    {
        /// <summary>
        /// Decodes the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The decrypted token as mail.</returns>
        /// <exception cref="ArgumentException">The token is invalid - token</exception>
        public static string GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                var userId = ((List<Claim>)jsonToken.Claims).First(p => p.Type.Equals("sub")).Value;
                return userId;
            }
            catch
            {
                throw new ArgumentException("The token is invalid", nameof(token));
            }
        }
    }
}
