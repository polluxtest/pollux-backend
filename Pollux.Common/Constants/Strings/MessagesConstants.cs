namespace Pollux.Common.Constants.Strings
{
    public static class MessagesConstants
    {
        /// <summary>
        /// The not authenticated message to be handled for api consumers.
        /// </summary>
        public const string NotAuthenticated = "Not Authenticated Expired Access Token 401";

        /// <summary>
        /// The expired session message
        /// </summary>
        public const string NotAuthenticatedSession = "Not Authenticated Expired Refresh Token 440";

        /// <summary>
        /// The un expected error message
        /// </summary>
        public const string UnExpectedError = "Unexpected Error";

        /// <summary>
        /// The email empty error
        /// </summary>
        public const string EmailEmptyError = "Email object is null";
    }
}
