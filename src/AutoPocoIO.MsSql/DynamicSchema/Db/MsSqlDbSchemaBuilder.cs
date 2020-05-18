using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Exceptions;
using AutoPocoIO.MsSql.DynamicSchema.Runtime;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;

namespace AutoPocoIO.DynamicSchema.Db
{
    internal class MsSqlDbSchemaBuilder : DbSchemaBuilderBase
    {
        private readonly MsSqlSchmeaQueries _query;

        public MsSqlDbSchemaBuilder(MsSqlSchmeaQueries query,
            Config config,
            IDbSchema schema,
            IDbTypeMapper typeMapper)
            : base(config, schema, typeMapper)
        {
            Check.NotNull(query, nameof(query));
            _query = query;
        }

        public override ResourceType ResourceType => ResourceType.Mssql;

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(Config.ConnectionString);
        }

        public override IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public override DbContextOptions CreateDbContextOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(Config.ConnectionString,
                   c => c.UseNetTopologySuite());

            optionsBuilder.ReplaceEFCachingServices();
            optionsBuilder.ReplaceSqlServerEFCachingServices();
            optionsBuilder.ReplaceEFCrossDbServices();
            optionsBuilder.ReplaceSqlServerEFCrossDbServices();

            return optionsBuilder.Options;
        }


        protected override IDbCommand BuildColumnsCommand(IDbConnection dbConnection)
        {
            SqlCommand Command = new SqlCommand
            {
                CommandType = CommandType.Text,
                Connection = (SqlConnection)dbConnection,
                CommandText = _query.BuildColumns()
            };

            return Command;
        }

        protected override IDbCommand BuildStoredProcedureCommand(IDbConnection dbConnection)
        {
            SqlCommand Command = new SqlCommand
            {
                CommandType = CommandType.Text,
                Connection = (SqlConnection)dbConnection,
                CommandText = _query.BuildStoredProcedureCommand()
            };

            return Command;
        }

        protected override IDbCommand BuildTablesViewsCommand(IDbConnection dbConnection)
        {
            SqlCommand Command = new SqlCommand
            {
                CommandType = CommandType.Text,
                Connection = (SqlConnection)dbConnection,
                CommandText = _query.BuildTablesViewCommand()
            };

            return Command;
        }
    }
}