namespace Pollux.Common.Application.Models.Request
{
    public class LogInModel
    {
        /// <summary>
        /// Gets or sets the User Id.
        /// </summary>
        /// <value>
        /// The User Id.
        /// </value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
    }
}
