using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class SqlServerQuerySqlGenerator : Microsoft.EntityFrameworkCore.SqlServer.Query.Sql.Internal.SqlServerQuerySqlGenerator
    {
        public SqlServerQuerySqlGenerator(
            QuerySqlGeneratorDependencies dependencies,
            Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression selectExpression,
           bool rowNumberPagingEnabled)
           : base(dependencies, selectExpression, rowNumberPagingEnabled)
        {
        }

        public override Expression VisitTable(Microsoft.EntityFrameworkCore.Query.Expressions.TableExpression tableExpression)
        {
            if (tableExpression is TableExpression tableExpressionWithDb)
                Sql.Append(SqlGenerator.DelimitIdentifier(tableExpressionWithDb.DatabaseName) + ".");

            Sql.Append(SqlGenerator.DelimitIdentifier(tableExpression.Table, tableExpression.Schema))
                .Append(AliasSeparator)
                .Append(SqlGenerator.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }
    }
}
