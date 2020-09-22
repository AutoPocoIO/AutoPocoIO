using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.Migrations
{
    public class AppMigrationContext : DbContext
    {
        public AppMigrationContext()
        {
        }

        public AppMigrationContext(DbContextOptions<AppMigrationContext> options) : base(options)
        {
        }
    }
}
