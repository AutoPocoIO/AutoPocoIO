using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPocoIO.EntityConfiguration
{
    internal class UserJoinEntityConfiguration : IEntityTypeConfiguration<UserJoin>
    {
        public void Configure(EntityTypeBuilder<UserJoin> builder)
        {
            builder.HasIndex(c => c.Alias)
                 .IsUnique();

            builder.Property(c => c.Id)
              .HasConversion<string>();

            builder.Property(c => c.FKConnectorId)
              .HasConversion<string>();

            builder.Property(c => c.PKConnectorId)
              .HasConversion<string>();
        }
    }
}
