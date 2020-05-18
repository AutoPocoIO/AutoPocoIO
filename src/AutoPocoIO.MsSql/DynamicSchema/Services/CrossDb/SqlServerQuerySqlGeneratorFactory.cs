using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class SqlServerQuerySqlGeneratorFactory : Microsoft.EntityFrameworkCore.Query.Sql.QuerySqlGeneratorFactoryBase
    {
        private readonly ISqlServerOptions _sqlServerOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerQuerySqlGeneratorFactory(
             QuerySqlGeneratorDependencies dependencies,
             ISqlServerOptions sqlServerOptions)
            : base(dependencies)
        {
            _sqlServerOptions = sqlServerOptions;
        }

        public override IQuerySqlGenerator CreateDefault(Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression selectExpression)
        {
            return new SqlServerQuerySqlGenerator(
                 Dependencies,
                 selectExpression,
                 _sqlServerOptions.RowNumberPagingEnabled);
        }
    }
}
