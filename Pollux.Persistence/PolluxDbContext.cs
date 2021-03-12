namespace Pollux.Persistence
{
    using System.Reflection;

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
        /// Gets or sets the <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> of roles.
        /// </summary>
        public new DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(AssemblyPersistence.Assembly);
            this.IgnoreTables(modelBuilder);
            this.RenameTables(modelBuilder);
        }

        /// <summary>
        /// Ignores the tables  of Identity Framework.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        private void IgnoreTables(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>();
        }

        /// <summary>
        /// Renames the tables Renames the tables of Identity Framework.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        private void RenameTables(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(
               p =>
               {
                   p.ToTable("UserRoles");
                   p.HasKey(y => new { y.UserId, y.RoleId });
               });
        }
    }
}
