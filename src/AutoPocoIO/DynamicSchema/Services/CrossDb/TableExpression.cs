using Remotion.Linq.Clauses;
using System.Diagnostics.CodeAnalysis;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [ExcludeFromCodeCoverage]
    public class TableExpression : Microsoft.EntityFrameworkCore.Query.Expressions.TableExpression
    {
        public TableExpression(string database, string table, string schema, string alias, IQuerySource querySource)
            : base(table, schema, alias, querySource)
        {
            DatabaseName = database;
        }

        public string DatabaseName { get; set; }
    }
}
