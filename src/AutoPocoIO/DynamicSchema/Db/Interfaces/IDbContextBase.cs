using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;

namespace AutoPocoIO.DynamicSchema.Db
{
    public interface IDbContextBase
    {
        IDbCommand CreateDbCommand();
        int SaveChanges();
        IModel Model { get; }
        DatabaseFacade Database { get; }
    }
}