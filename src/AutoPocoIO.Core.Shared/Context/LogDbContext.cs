using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.Context
{

    public class LogDbContext : DbContext
    {
        private readonly IContextEntityConfiguration _config;

        public LogDbContext(DbContextOptions<LogDbContext> options, IContextEntityConfiguration config) : base(options)
        {
            _config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _config.SetupLogDbContext(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<RequestLog> RequestLogs { get; set; }
        public virtual DbSet<ResponseLog> ResponseLogs { get; set; }
    }
}
