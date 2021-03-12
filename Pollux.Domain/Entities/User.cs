namespace Pollux.Domain.Entities
{
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// User Entity.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.IdentityUser" />
    public class User : IdentityUser<string>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
    }
}
