using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.EntityConfiguration
{
    public interface IContextEntityConfiguration
    {
        void SetupAppDbContext(ModelBuilder modelBuilder);
        void SetupLogDbContext(ModelBuilder modelBuilder);
    }
}
