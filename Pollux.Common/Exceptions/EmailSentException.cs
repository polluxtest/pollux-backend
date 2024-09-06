namespace Pollux.Common.Exceptions
{
    using System;

    /// <summary>
    /// Email Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class EmailSentException : Exception
    {
        public EmailSentException()
        {
        }

        public EmailSentException(string message)
            : base(message)
        {
        }
    }
}
