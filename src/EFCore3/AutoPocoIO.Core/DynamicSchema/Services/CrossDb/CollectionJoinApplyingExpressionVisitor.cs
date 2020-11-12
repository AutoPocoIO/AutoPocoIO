﻿using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class CollectionJoinApplyingExpressionVisitor : Microsoft.EntityFrameworkCore.Query.Internal.CollectionJoinApplyingExpressionVisitor
    {
        private int _collectionId;

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is CollectionShaperExpression collectionShaperExpression)
            {
                var collectionId = _collectionId++;
                var projectionBindingExpression = (ProjectionBindingExpression)collectionShaperExpression.Projection;
                var selectExpression = (SelectExpression)projectionBindingExpression.QueryExpression;
                // Do pushdown beforehand so it updates all pending collections first
                if (selectExpression.IsDistinct
                    || selectExpression.Limit != null
                    || selectExpression.Offset != null
                    || selectExpression.GroupBy.Count > 1)
                {
                    selectExpression.PushdownIntoSubquery();
                }

                var innerShaper = Visit(collectionShaperExpression.InnerShaper);

                return selectExpression.ApplyCollectionJoin(
                    projectionBindingExpression.Index.Value,
                    collectionId,
                    innerShaper,
                    collectionShaperExpression.Navigation,
                    collectionShaperExpression.ElementType);
            }

            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
                shapedQueryExpression.ShaperExpression = Visit(shapedQueryExpression.ShaperExpression);

                return shapedQueryExpression;
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
