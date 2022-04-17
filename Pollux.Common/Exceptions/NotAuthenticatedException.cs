namespace Pollux.Common.Exceptions
{
    using System;

    public class NotAuthenticatedException : Exception
    {
        public NotAuthenticatedException()
        {
        }

        public NotAuthenticatedException(string message)
            : base(message)
        {
        }
    }
}
