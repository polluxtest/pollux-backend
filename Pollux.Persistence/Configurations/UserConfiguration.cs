namespace Pollux.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Pollux.Domain.Entities;

    /// <summary>
    /// User db Configuration.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{Pollux.Domain.Entities.User}" />
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// Configures the entity of type <typeparamref name="TEntity" />.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.Property(p => p.Name).IsRequired().HasMaxLength(20);
            builder.Property(p => p.Email).IsRequired().HasMaxLength(50);
            builder.Property(p => p.EmailConfirmed).HasDefaultValue(false);
            builder.Property(p => p.PhoneNumber).IsRequired(false);
            builder.Property(p => p.UserName).IsRequired(false);

            builder.Ignore(p => p.PhoneNumberConfirmed);
            builder.Ignore(p => p.AccessFailedCount);
            builder.Ignore(p => p.LockoutEnabled);

            builder.Ignore(p => p.TwoFactorEnabled);
            builder.Ignore(p => p.ConcurrencyStamp);
            builder.Ignore(p => p.LockoutEnd);
            builder.Ignore(p => p.NormalizedEmail);

            builder.HasIndex(p => p.Id);
            builder.HasIndex(p => p.Email).IsUnique().IncludeProperties("Id", "Name", "UserName");
        }
    }
}
