using System;
using System.Collections.Generic;
using System.Text;

namespace Pollux.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using Pollux.Domain.Entities;

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Ignore(p => p.NormalizedName);
            builder.Ignore(p => p.ConcurrencyStamp);
        }
    }
}
