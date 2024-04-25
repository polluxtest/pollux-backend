namespace Pollux.Common.Constants
{
    public class ExpirationConstants
    {
        /// <summary>
        /// The refresh token expiration in seconds 30 days
        /// </summary>
        public const int RefreshTokenExpirationSeconds = 2592000;

        /// <summary>
        /// The access token expiration seconds 1 day.
        /// </summary>
        public const int AccessTokenExpiratioSeconds = 86400;

        /// <summary>
        /// The redis cache expiration seconds 30 days.
        /// </summary>
        public const int RedisCacheExpirationSeconds = 2592000;
    }
}
