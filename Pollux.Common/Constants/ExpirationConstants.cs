namespace Pollux.Common.Constants
{
    public class ExpirationConstants
    {
        /// <summary>
        /// The refresh token expiration in seconds 30 days
        /// </summary>
        public const int RefreshTokenExpirationSeconds = 2592000;

        /// <summary>
        /// The access token expiration seconds 10 day.
        /// </summary>
        public const int AccessTokenExpiratioSeconds = 864000;

        /// <summary>
        /// The redis cache expiration seconds 31 days.
        /// </summary>
        public const int RedisCacheExpirationSeconds = 2678400;
    }
}
