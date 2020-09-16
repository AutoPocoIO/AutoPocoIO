using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq.Clauses;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class RelationalEntityQueryableExpressionVisitorFactory : Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.RelationalEntityQueryableExpressionVisitorFactory
    {
        public RelationalEntityQueryableExpressionVisitorFactory(
           RelationalEntityQueryableExpressionVisitorDependencies dependencies) : base(dependencies)
        {
        }

        public override ExpressionVisitor Create(EntityQueryModelVisitor queryModelVisitor, IQuerySource querySource)
        {
            return new RelationalEntityQueryableExpressionVisitor(
                    Dependencies,
                    (RelationalQueryModelVisitor)queryModelVisitor,
                    querySource);
        }
    }
}
