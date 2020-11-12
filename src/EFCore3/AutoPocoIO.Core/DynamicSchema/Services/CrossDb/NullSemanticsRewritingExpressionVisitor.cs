using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class NullSemanticsRewritingExpressionVisitor : Microsoft.EntityFrameworkCore.Query.Internal.NullSemanticsRewritingExpressionVisitor
    {
        protected readonly ISqlExpressionFactoryWithCrossDb _sqlExpressionFactory;
        private bool _canOptimize;
        private bool _isNullable;

        public NullSemanticsRewritingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
            : base(sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory as ISqlExpressionFactoryWithCrossDb;
            _canOptimize = true;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case ExceptExpression exceptExpression:
                    return VisitExcept(exceptExpression);
                case ExistsExpression existsExpression:
                    return VisitExists(existsExpression);
                case InExpression inExpression:
                    return VisitIn(inExpression);
                case IntersectExpression intersectExpression:
                    return VisitIntersect(intersectExpression);
                case SelectExpression selectExpression:
                    return VisitSelect(selectExpression);
                case ScalarSubqueryExpression scalarSubqueryExpression:
                    return VisitSubSelect(scalarSubqueryExpression);
                case TableExpression tableExpression:
                    return VisitTable(tableExpression);
                case UnionExpression unionExpression:
                    return VisitUnion(unionExpression);

            }
            return base.VisitExtension(extensionExpression);
        }

        protected Expression VisitExcept(ExceptExpression exceptExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var source1 = (SelectExpression)Visit(exceptExpression.Source1);
            var source2 = (SelectExpression)Visit(exceptExpression.Source2);
            _canOptimize = canOptimize;

            return exceptExpression.Update(source1, source2);
        }

        protected Expression VisitExists(ExistsExpression existsExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var newSubquery = (SelectExpression)Visit(existsExpression.Subquery);
            _canOptimize = canOptimize;

            return existsExpression.Update(newSubquery);
        }

        protected Expression VisitIn(InExpression inExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            _isNullable = false;
            var item = (SqlExpression)Visit(inExpression.Item);
            var isNullable = _isNullable;
            _isNullable = false;
            var subquery = (SelectExpression)Visit(inExpression.Subquery);
            isNullable |= _isNullable;
            _isNullable = false;
            var values = (SqlExpression)Visit(inExpression.Values);
            _isNullable |= isNullable;
            _canOptimize = canOptimize;

            return inExpression.Update(item, values, subquery);
        }

        protected Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var source1 = (SelectExpression)Visit(intersectExpression.Source1);
            var source2 = (SelectExpression)Visit(intersectExpression.Source2);
            _canOptimize = canOptimize;

            return intersectExpression.Update(source1, source2);
        }

        protected Expression VisitSelect(SelectExpression selectExpression)
        {
            var changed = false;
            var canOptimize = _canOptimize;
            var projections = new List<ProjectionExpression>();
            _canOptimize = false;
            foreach (var item in selectExpression.Projection)
            {
                var updatedProjection = (ProjectionExpression)Visit(item);
                projections.Add(updatedProjection);
                changed |= updatedProjection != item;
            }

            var tables = new List<TableExpressionBase>();
            foreach (var table in selectExpression.Tables)
            {
                var newTable = (TableExpressionBase)Visit(table);
                changed |= newTable != table;
                tables.Add(newTable);
            }

            _canOptimize = true;
            var predicate = (SqlExpression)Visit(selectExpression.Predicate);
            changed |= predicate != selectExpression.Predicate;

            var groupBy = new List<SqlExpression>();
            _canOptimize = false;
            foreach (var groupingKey in selectExpression.GroupBy)
            {
                var newGroupingKey = (SqlExpression)Visit(groupingKey);
                changed |= newGroupingKey != groupingKey;
                groupBy.Add(newGroupingKey);
            }

            _canOptimize = true;
            var havingExpression = (SqlExpression)Visit(selectExpression.Having);
            changed |= havingExpression != selectExpression.Having;

            var orderings = new List<OrderingExpression>();
            _canOptimize = false;
            foreach (var ordering in selectExpression.Orderings)
            {
                var orderingExpression = (SqlExpression)Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var offset = (SqlExpression)Visit(selectExpression.Offset);
            changed |= offset != selectExpression.Offset;

            var limit = (SqlExpression)Visit(selectExpression.Limit);
            changed |= limit != selectExpression.Limit;

            _canOptimize = canOptimize;

            // we assume SelectExpression can always be null
            // (e.g. projecting non-nullable column but with predicate that filters out all rows)
            _isNullable = true;

            return changed
                ? selectExpression.Update(
                    projections, tables, predicate, groupBy, havingExpression, orderings, limit, offset, selectExpression.IsDistinct,
                    selectExpression.Alias)
                : selectExpression;
        }

        protected Expression VisitSubSelect(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var subquery = (SelectExpression)Visit(scalarSubqueryExpression.Subquery);
            _canOptimize = canOptimize;

            return scalarSubqueryExpression.Update(subquery);
        }

        protected Expression VisitTable(TableExpression tableExpression)
            => tableExpression;

        protected Expression VisitUnion(UnionExpression unionExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var source1 = (SelectExpression)Visit(unionExpression.Source1);
            var source2 = (SelectExpression)Visit(unionExpression.Source2);
            _canOptimize = canOptimize;

            return unionExpression.Update(source1, source2);
        }
    }
}
