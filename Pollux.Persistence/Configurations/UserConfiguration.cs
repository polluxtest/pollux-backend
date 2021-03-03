namespace Pollux.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Pollux.Domain.Entities;

    /// <summary>
    /// User db Configuration
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{Pollux.Domain.Entities.User}" />
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            builder.Ignore(p => p.Claims);
            builder.Ignore(p => p.LockoutEndDateUtc);
            builder.Ignore(p => p.LockoutEnabled);
            builder.Ignore(p => p.Logins);
            builder.Ignore(p => p.SecurityStamp);
            builder.Ignore(p => p.TwoFactorEnabled);
            builder.HasIndex("Id");
        }
    }
}
