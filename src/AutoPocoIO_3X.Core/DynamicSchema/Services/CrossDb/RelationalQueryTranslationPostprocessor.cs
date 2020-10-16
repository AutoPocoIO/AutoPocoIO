using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal class RelationalQueryTranslationPostprocessor : Microsoft.EntityFrameworkCore.Query.RelationalQueryTranslationPostprocessor
    {
        private readonly SqlExpressionOptimizingExpressionVisitor _sqlExpressionOptimizingExpressionVisitor;
        protected readonly ISqlExpressionFactoryWithCrossDb _sqlExpressionFactory;

        public RelationalQueryTranslationPostprocessor(
           QueryTranslationPostprocessorDependencies dependencies,
           RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
           QueryCompilationContext queryCompilationContext)
           : base(dependencies, relationalDependencies, queryCompilationContext)
        {
            _sqlExpressionOptimizingExpressionVisitor
                = new SqlExpressionOptimizingExpressionVisitor(SqlExpressionFactory, UseRelationalNulls);

            _sqlExpressionFactory = SqlExpressionFactory as ISqlExpressionFactoryWithCrossDb;
        }

        public override Expression Process(Expression query)
        {
            query = new SelectExpressionProjectionApplyingExpressionVisitor().Visit(query);
            query = new CollectionJoinApplyingExpressionVisitor().Visit(query);
            query = new TableAliasUniquifyingExpressionVisitor().Visit(query);

            if (!(AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue12729", out var enabled) && enabled))
            {
                //query = new CaseWhenFlatteningExpressionVisitor(SqlExpressionFactory).Visit(query);
            }

            if (!UseRelationalNulls)
            {
                query = new NullSemanticsRewritingExpressionVisitor(SqlExpressionFactory).Visit(query);
            }

            query = OptimizeSqlExpression(query);

            return query;
        }

        protected override Expression OptimizeSqlExpression(Expression query) => _sqlExpressionOptimizingExpressionVisitor.Visit(query);
    }
}
