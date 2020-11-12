using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [ExcludeFromCodeCoverage]
    public class ExistsExpression : SqlExpression
    {
        public ExistsExpression(SelectExpression subquery, bool negated, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Subquery = subquery;
            IsNegated = negated;
        }

        public virtual SelectExpression Subquery { get; }
        public virtual bool IsNegated { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SelectExpression)visitor.Visit(Subquery));

        public virtual ExistsExpression Update(SelectExpression subquery)
            => subquery != Subquery
                ? new ExistsExpression(subquery, IsNegated, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            if (IsNegated)
            {
                expressionPrinter.Append("NOT ");
            }

            expressionPrinter.AppendLine("EXISTS (");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Subquery);
            }

            expressionPrinter.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ExistsExpression existsExpression
                    && Equals(existsExpression));

        private bool Equals(ExistsExpression existsExpression)
            => base.Equals(existsExpression)
                && Subquery.Equals(existsExpression.Subquery)
                && IsNegated == existsExpression.IsNegated;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Subquery, IsNegated);
    }
}
