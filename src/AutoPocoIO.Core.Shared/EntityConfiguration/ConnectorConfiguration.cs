using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPocoIO.EntityConfiguration
{
    internal class ConnectorConfiguration : IEntityTypeConfiguration<Connector>
    {
        public void Configure(EntityTypeBuilder<Connector> builder)
        {
            builder.HasIndex(c => c.Name)
                 .IsUnique()
                 .HasName("IDX_ConnectorName");

            builder.Property(c => c.Id)
               .HasConversion<string>();
        }
    }
}
