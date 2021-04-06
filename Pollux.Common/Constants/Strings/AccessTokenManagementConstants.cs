namespace Pollux.Common.Constants.Strings
{
    /// <summary>
    /// Default values for token identity server
    /// </summary>
    public static class AccessTokenManagementConstants
    {

        /// <summary>
        /// Name of the back-channel HTTP client
        /// </summary>
        public const string BackChannelHttpClientName = "IdentityModel.AspNetCore.AccessTokenManagement.TokenEndpointService";

        /// <summary>
        /// Name used to propagate access token parameters to HttpRequestMessage
        /// </summary>
        public const string AccessTokenParametersOptionsName = "IdentityModel.AspNetCore.AccessTokenParameters";
    }
}