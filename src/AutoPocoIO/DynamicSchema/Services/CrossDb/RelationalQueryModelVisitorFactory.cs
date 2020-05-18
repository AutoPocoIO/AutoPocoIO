using Microsoft.EntityFrameworkCore.Query;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class RelationalQueryModelVisitorFactory : Microsoft.EntityFrameworkCore.Query.RelationalQueryModelVisitorFactory
    {
        public RelationalQueryModelVisitorFactory(EntityQueryModelVisitorDependencies dependencies, RelationalQueryModelVisitorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public override EntityQueryModelVisitor Create(QueryCompilationContext queryCompilationContext, EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            return new RelationalQueryModelVisitor(
                  Dependencies,
                  RelationalDependencies,
                  (RelationalQueryCompilationContext)queryCompilationContext,
                  (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);
        }
    }
}
