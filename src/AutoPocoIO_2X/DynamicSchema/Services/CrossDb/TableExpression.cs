using Remotion.Linq.Clauses;
using System.Diagnostics.CodeAnalysis;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    /// <summary>
    /// Add database name to table expressions
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TableExpression : Microsoft.EntityFrameworkCore.Query.Expressions.TableExpression
    {
        /// <summary>
        /// Initialize table expression with database name.
        /// </summary>
        /// <param name="database">Database name</param>
        /// <param name="table">Table name.</param>
        /// <param name="schema">Schema name.</param>
        /// <param name="alias">Alias name.</param>
        /// <param name="querySource">The query source.</param>
        public TableExpression(string database, string table, string schema, string alias, IQuerySource querySource)
            : base(table, schema, alias, querySource)
        {
            DatabaseName = database;
        }

        /// <summary>
        /// Database name for expression
        /// </summary>
        public string DatabaseName { get; set; }
    }
}
