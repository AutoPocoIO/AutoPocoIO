﻿using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class SelectExpressionProjectionApplyingExpressionVisitor : Microsoft.EntityFrameworkCore.Query.Internal.SelectExpressionProjectionApplyingExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is SelectExpression selectExpression)
            {
                selectExpression.ApplyProjection();
            }

            return base.VisitExtension(node);
        }
    }
}
