namespace Pollux.Common.Constants
{
    public class ExpirationConstants
    {
        /// <summary>
        /// The refresh token expiration in days.
        /// </summary>
        public const int RefreshTokenExpirationDays = 30;

        /// <summary>
        /// The refresh token expiration seconds.
        /// </summary>
        public const int RefreshTokenExpirationSeconds = 300;

        /// <summary>
        /// The access token expiration in days.
        /// </summary>
        public const int AccessTokenExpirationDays = 3;

        /// <summary>
        /// The access token expiration seconds.
        /// </summary>
        public const int AccessTokenExpirationSeconds = 120;

        /// <summary>
        /// The redis cache expiration seconds.
        /// </summary>
        public const int RedisCacheExpirationSeconds = 300;

        /// <summary>
        /// The session expiration in seconds.
        /// </summary>
        public const int SessionExpiration = 600;
    }
}
