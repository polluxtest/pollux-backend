namespace Pollux.Common.Constants
{
    public class ExpirationConstants
    {
        /// <summary>
        /// The refresh token expiration in days.
        /// </summary>
        public const int RefreshTokenExpirationDays = 20;

        /// <summary>
        /// The refresh token expiration seconds 20 days.
        /// </summary>
        public const int RefreshTokenExpirationSeconds = 1728000;

        /// <summary>
        /// The access token expiration in days.
        /// </summary>
        public const int AccessTokenExpirationDays = 10;

        /// <summary>
        /// The access token expiration seconds 10 days.
        /// </summary>
        public const int AccessTokenExpirationSeconds = 864000;

        /// <summary>
        /// The redis cache expiration seconds 20 days.
        /// </summary>
        public const int RedisCacheExpirationSeconds = 1728000;

        /// <summary>
        /// The session expiration in seconds 20 days.
        /// </summary>
        public const int SessionExpiration = 1728000;
    }
}
