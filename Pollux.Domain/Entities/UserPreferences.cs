namespace Pollux.Domain.Entities
{
    public class UserPreferences
    {
        /// <summary>Gets or sets the user identifier.</summary>
        /// <value>The user identifier.</value>
        public string UserId { get; set; }

        /// <summary>Gets or sets the user.</summary>
        /// <value>The user.</value>
        public User User { get; set; }

        /// <summary>Gets or sets the key.</summary>
        /// <value>The key.</value>
        public string Key { get; set; }

        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public string Value { get; set; }
    }
}
