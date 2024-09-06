namespace Pollux.Common.Application.Models.Request
{
    public class ResetPasswordModel
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }

        /// <summary>
        /// Creates new password.
        /// </summary>
        /// <value>
        /// The new password.
        /// </value>
        public string NewPassword { get; set; }
    }
}
