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
        }
    }
}
