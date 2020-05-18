using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPocoIO.EntityConfiguration
{
    internal class RequestLogConfiguration : IEntityTypeConfiguration<RequestLog>
    {

        public void Configure(EntityTypeBuilder<RequestLog> builder)
        {
            builder.HasKey(c => new { c.RequestId, c.RequestGuid });

            //Indexes
            builder.ForSqlServerHasIndex(c => c.RequestGuid)
                .ForSqlServerInclude(c => new { c.DateTimeUtc, c.RequestType, c.RequesterIp, c.Connector });

            builder.ForSqlServerHasIndex(c => c.DayOfRequest)
               .HasName("IX_DayWithIP")
               .ForSqlServerInclude(c => new { c.RequesterIp });

            builder.HasIndex(c => c.DateTimeUtc);

            builder.HasIndex(c => c.DayOfRequest);

            builder.HasIndex(c => new { c.DayOfRequest, c.RequestType })
                 .HasName("IX_DayAndType");

            builder.Property("DayOfRequest")
                .HasComputedColumnSql("CONVERT(date, DateTimeUtc)");
        }
    }
}
