using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.Context
{

    public class LogDbContext : DbContext
    {
        private IContextEntityConfiguration _config;

        public LogDbContext(DbContextOptions<LogDbContext> options, IContextEntityConfiguration config) : base(options)
        {
            _config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _config.SetupLogDbContext(modelBuilder);
            //modelBuilder.ApplyConfiguration(new RequestLogConfiguration());
            //modelBuilder.ApplyConfiguration(new ResponseLogConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<RequestLog> RequestLogs { get; set; }
        public virtual DbSet<ResponseLog> ResponseLogs { get; set; }
    }
}
