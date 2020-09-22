using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.Context
{

    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.ApplyConfiguration(new RequestLogConfiguration());
            //modelBuilder.ApplyConfiguration(new ResponseLogConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<RequestLog> RequestLogs { get; set; }
        public virtual DbSet<ResponseLog> ResponseLogs { get; set; }
    }
}
