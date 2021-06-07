using System;

namespace Pollux.Common.Exceptions
{
    public class NotAuthenticatedException : Exception
    {
        public NotAuthenticatedException(string message)
            : base(message)
        {
        }
    }
}
