using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPocoIO.EntityConfiguration
{
    internal class UserJoinConfiguration : IEntityTypeConfiguration<UserJoin>
    {
        public void Configure(EntityTypeBuilder<UserJoin> builder)
        {
            builder.HasIndex(c => c.Alias)
                 .IsUnique();
        }
    }
}
