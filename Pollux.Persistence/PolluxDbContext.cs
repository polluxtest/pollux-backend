namespace Pollux.Persistence
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Pollux.Domain.Entities;

    /// <summary>
    /// Core Domain Db Context.
    /// </summary>
    public class PolluxDbContext : IdentityDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolluxDbContext"/> class.
        /// </summary>
        /// <param name="options">The options<see cref="DbContextOptions{PolluxDbContext}"/>.</param>
        public PolluxDbContext(DbContextOptions<PolluxDbContext> options)
          : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        public new DbSet<User> Users { get; set; }

        /// <summary>
        /// The OnConfiguring.
        /// </summary>
        /// <param name="optionsBuilder">The optionsBuilder<see cref="DbContextOptionsBuilder"/>.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
