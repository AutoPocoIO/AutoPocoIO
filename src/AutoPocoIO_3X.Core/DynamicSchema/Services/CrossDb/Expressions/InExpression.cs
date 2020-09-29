﻿using System;
using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    public class InExpression : SqlExpression
    {
        public InExpression(SqlExpression item, bool negated, SelectExpression subquery, RelationalTypeMapping typeMapping)
            : this(item, negated, null, subquery, typeMapping)
        {
        }

        public InExpression(SqlExpression item, bool negated, SqlExpression values, RelationalTypeMapping typeMapping)
            : this(item, negated, values, null, typeMapping)
        {
        }

        private InExpression(
            SqlExpression item, bool negated, SqlExpression values, SelectExpression subquery,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Item = item;
            IsNegated = negated;
            Subquery = subquery;
            Values = values;
        }

        public virtual SqlExpression Item { get; }
        public virtual bool IsNegated { get; }
        public virtual SqlExpression Values { get; }
        public virtual SelectExpression Subquery { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newItem = (SqlExpression)visitor.Visit(Item);
            var subquery = (SelectExpression)visitor.Visit(Subquery);
            var values = (SqlExpression)visitor.Visit(Values);

            return Update(newItem, values, subquery);
        }

        public virtual InExpression Negate() => new InExpression(Item, !IsNegated, Values, Subquery, TypeMapping);

        public virtual InExpression Update(SqlExpression item, SqlExpression values, SelectExpression subquery)
            => item != Item || subquery != Subquery || values != Values
                ? new InExpression(item, IsNegated, values, subquery, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Visit(Item);
            expressionPrinter.Append(IsNegated ? " NOT IN " : " IN ");
            expressionPrinter.Append("(");

            if (Values is SqlConstantExpression constantValuesExpression
                && constantValuesExpression.Value is IEnumerable constantValues)
            {
                var first = true;
                foreach (var item in constantValues)
                {
                    if (!first)
                    {
                        expressionPrinter.Append(", ");
                    }

                    first = false;
                    expressionPrinter.Append(constantValuesExpression.TypeMapping?.GenerateSqlLiteral(item) ?? item?.ToString() ?? "NULL");
                }
            }
            else
            {
                expressionPrinter.Visit(Values);
            }

            expressionPrinter.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is InExpression inExpression
                    && Equals(inExpression));

        private bool Equals(InExpression inExpression)
            => base.Equals(inExpression)
                && Item.Equals(inExpression.Item)
                && IsNegated.Equals(inExpression.IsNegated)
                && (Values == null ? inExpression.Values == null : Values.Equals(inExpression.Values))
                && (Subquery == null ? inExpression.Subquery == null : Subquery.Equals(inExpression.Subquery));

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Item, IsNegated, Values, Subquery);
    }
}
