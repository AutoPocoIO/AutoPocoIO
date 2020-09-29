using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    /// <summary>
    /// Cross db override
    /// </summary>
    internal class SqlExpressionOptimizingExpressionVisitor : Microsoft.EntityFrameworkCore.Query.Internal.SqlExpressionOptimizingExpressionVisitor
    {
        protected readonly ISqlExpressionFactoryWithCrossDb _sqlExpressionFactory;

        public SqlExpressionOptimizingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory, bool useRelationalNulls)
            : base(sqlExpressionFactory, useRelationalNulls)
        {
            _sqlExpressionFactory = sqlExpressionFactory as ISqlExpressionFactoryWithCrossDb;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
          => extensionExpression switch
          {
              SelectExpression selectExpression => VisitSelectExpression(selectExpression),
              _ => base.VisitExtension(extensionExpression),
          };

        private Expression VisitSelectExpression(SelectExpression selectExpression)
        {
            var newExpression = base.VisitExtension(selectExpression);

            // if predicate is optimized to true, we can simply remove it
            if (newExpression is SelectExpression newSelectExpression)
            {
                var changed = false;
                var newPredicate = newSelectExpression.Predicate;
                var newHaving = newSelectExpression.Having;
                if (newSelectExpression.Predicate is SqlConstantExpression predicateConstantExpression
                    && predicateConstantExpression.Value is bool predicateBoolValue
                    && predicateBoolValue)
                {
                    newPredicate = null;
                    changed = true;
                }

                if (newSelectExpression.Having is SqlConstantExpression havingConstantExpression
                    && havingConstantExpression.Value is bool havingBoolValue
                    && havingBoolValue)
                {
                    newHaving = null;
                    changed = true;
                }

                return changed
                    ? newSelectExpression.Update(
                        newSelectExpression.Projection.ToList(),
                        newSelectExpression.Tables.ToList(),
                        newPredicate,
                        newSelectExpression.GroupBy.ToList(),
                        newHaving,
                        newSelectExpression.Orderings.ToList(),
                        newSelectExpression.Limit,
                        newSelectExpression.Offset,
                        newSelectExpression.IsDistinct,
                        newSelectExpression.Alias)
                    : newSelectExpression;
            }

            return newExpression;
        }
    }
}
