using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPocoIO.EntityConfiguration
{
    internal class ResponseLogConfiguration : IEntityTypeConfiguration<ResponseLog>
    {
        public void Configure(EntityTypeBuilder<ResponseLog> builder)
        {
            builder.HasKey(c => new { c.ResponseId, c.RequestGuid });

            builder.Property("DayOfResponse")
                .HasComputedColumnSql("CONVERT(date, DateTimeUtc)");
        }
    }
}
