using AutoPocoIO.DynamicSchema.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AutoPocoIO.DynamicSchema.Db
{
    public interface IDbSchemaBuilder
    {
        ResourceType ResourceType { get; }

        IDbConnection CreateConnection();
        IDbConnection CreateConnection(string connectionString);
        DbContextOptions CreateDbContextOptions();
        DataTable ExecuteSchemaCommand(IDbCommand command);
        void GetColumns();
        void GetTableViews();
        void GetStoredProcedures();
    }
}