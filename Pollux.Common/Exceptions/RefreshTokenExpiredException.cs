namespace Pollux.Common.Exceptions
{
    using System;

    /// <summary>
    /// The Refresh Token Expiration Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class RefreshTokenExpiredException : Exception
    {
        public RefreshTokenExpiredException()
        {
        }

        public RefreshTokenExpiredException(string message)
            : base(message)
        {
        }
    }
}
