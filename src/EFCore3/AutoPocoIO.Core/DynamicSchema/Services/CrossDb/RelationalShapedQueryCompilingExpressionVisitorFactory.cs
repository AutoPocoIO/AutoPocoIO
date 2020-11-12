using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class RelationalShapedQueryCompilingExpressionVisitorFactory : Microsoft.EntityFrameworkCore.Query.Internal.RelationalShapedQueryCompilingExpressionVisitorFactory
    {
        private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;
        private readonly RelationalShapedQueryCompilingExpressionVisitorDependencies _relationalDependencies;

        public RelationalShapedQueryCompilingExpressionVisitorFactory(ShapedQueryCompilingExpressionVisitorDependencies dependencies,
                                                                      RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies) 
            : base(dependencies, relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public override ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
           => new RelationalShapedQueryCompilingExpressionVisitor(
               _dependencies,
               _relationalDependencies,
               queryCompilationContext);
    }
}
