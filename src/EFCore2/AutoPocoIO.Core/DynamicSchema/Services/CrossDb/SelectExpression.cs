using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class SelectExpression : Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression
    {
        public SelectExpression(SelectExpressionDependencies dependencies, RelationalQueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
        }

        public SelectExpression(SelectExpressionDependencies dependencies, RelationalQueryCompilationContext queryCompilationContext, string alias)
            : base(dependencies, queryCompilationContext, alias)
        {
        }

        /*  public override PredicateJoinExpressionBase AddInnerJoin(TableExpressionBase tableExpression)
          {
              return AddInnerJoin(tableExpression, Enumerable.Empty<AliasExpression>(), innerPredicate: null);
          }

          public override PredicateJoinExpressionBase AddInnerJoin(TableExpressionBase tableExpression, IEnumerable<Expression> projection, Expression innerPredicate)
          {
              var innerJoinExpression = new InnerJoinExpression(tableExpression);

              Tables.ToList().Add(innerJoinExpression);
              Projection.ToList().AddRange(projection);

              if (innerPredicate != null)
              {
                  AddToPredicate(innerPredicate);
              }

              return innerJoinExpression;
          }
          public override PredicateJoinExpressionBase AddLeftOuterJoin(TableExpressionBase tableExpression)
          {

              return AddLeftOuterJoin(tableExpression, Enumerable.Empty<AliasExpression>());
          }

          /// <summary>
          ///     Adds a SQL LEFT OUTER JOIN to this SelectExpression.
          /// </summary>
          /// <param name="tableExpression"> The target table expression. </param>
          /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
          public override PredicateJoinExpressionBase AddLeftOuterJoin(
               TableExpressionBase tableExpression,
               IEnumerable<Expression> projection)
          {

              var outerJoinExpression = new LeftOuterJoinExpression(tableExpression);

              Tables.ToList().Add(outerJoinExpression);
              Projection.ToList().AddRange(projection);

              return outerJoinExpression;
          }*/

        public override void RemoveTable(TableExpressionBase tableExpression)
        {
            //base.RemoveTable(tableExpression);
        }

        // public void PubClearGroupBy() => GroupBy. = new List<object);>
    }
}
