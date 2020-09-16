using AutoPocoIO.Exceptions;
using Microsoft.EntityFrameworkCore.Query;
using System.Diagnostics.CodeAnalysis;


namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    /// <summary>
    /// Add database name to table expressions
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TableExpression : Microsoft.EntityFrameworkCore.Query.SqlExpressions.TableExpressionBase
    {
        /// <summary>
        /// Initialize table expression with database name.
        /// </summary>
        /// <param name="database">Database name</param>
        /// <param name="table">Table name.</param>
        /// <param name="schema">Schema name.</param>
        /// <param name="alias">Alias name.</param>
        public TableExpression(string database, string table, string schema, string alias)
            : base(alias)
        {
            Name = table;
            Schema = schema;
            DatabaseName = database;
        }

        public string Name { get; }
        public string Schema { get; }
        /// <summary>
        /// Database name for expression
        /// </summary>
        public string DatabaseName { get; }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));


            if (!string.IsNullOrEmpty(DatabaseName))
            {
                expressionPrinter.Append(DatabaseName).Append(".");
            }

            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }
            else
            {
                expressionPrinter.Append("dbo.");
            }

            expressionPrinter.Append(Name).Append(" AS ").Append(Alias);
        }
    }
}