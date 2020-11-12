using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class SelectExpressionFactory : Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpressionFactory
    {
        public SelectExpressionFactory(SelectExpressionDependencies dependencies) : base(dependencies)
        {
        }

        public override Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext)
        {
            return new SelectExpression(Dependencies, queryCompilationContext);
        }

        public override Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext, string alias)
        {
            return new SelectExpression(Dependencies, queryCompilationContext, alias);
        }
    }
}
