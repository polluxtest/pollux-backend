using Polly.Bulkhead;

namespace Pollux.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Pollux.Domain.Entities;

    internal class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
    {
        public void Configure(EntityTypeBuilder<UserPreferences> builder)
        {
            builder.HasKey(p => new { p.UserId, p.Key });

            builder.Property(p => p.Key).HasMaxLength(20).IsRequired();
            builder.Property(p => p.Value).HasMaxLength(20).IsRequired();

            builder.HasIndex("UserId", "Key");
            builder.HasIndex(p => p.Key);
        }
    }
}
