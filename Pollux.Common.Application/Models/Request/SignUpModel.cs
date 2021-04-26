namespace Pollux.Common.Application.Models.Request
{
    public class SignUpModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the pass word.
        /// </summary>
        /// <value>
        /// The pass word.
        /// </value>
        public string Password { get; set; }
    }
}
