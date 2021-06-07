namespace Pollux.Common.Application.Models.Auth
{
    using System;
    public class TokenModel
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        public DateTime AccessTokenExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the refresh token expiration date.
        /// </summary>
        /// <value>
        /// The refresh token expiration date.
        /// </value>
        public DateTime RefreshTokenExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
    }
}
