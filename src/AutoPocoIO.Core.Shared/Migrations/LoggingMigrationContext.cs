using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.Migrations
{
    public class LoggingMigrationContext : DbContext
    {
        public LoggingMigrationContext()
        {
        }

        public LoggingMigrationContext(DbContextOptions<LoggingMigrationContext> options) : base(options)
        {
        }
    }
}
