namespace Pollux.Domain.Entities
{
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// User Entity.
    /// </summary>
    /// <seealso cref="Microsoft.AspNet.Identity.IdentityUser" />
    public class User : IdentityUser<string>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }
    }
}
