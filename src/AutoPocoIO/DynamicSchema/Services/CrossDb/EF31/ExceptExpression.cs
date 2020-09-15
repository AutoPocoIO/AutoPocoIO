using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal class ExceptExpression : SetOperationBase
    {
        public ExceptExpression(string alias, SelectExpression source1, SelectExpression source2, bool distinct)
            : base(alias, source1, source2, distinct)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var source1 = (SelectExpression)visitor.Visit(Source1);
            var source2 = (SelectExpression)visitor.Visit(Source2);

            return Update(source1, source2);
        }

        public virtual ExceptExpression Update(SelectExpression source1, SelectExpression source2)
            => source1 != Source1 || source2 != Source2
                ? new ExceptExpression(Alias, source1, source2, IsDistinct)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Visit(Source1);
                expressionPrinter.AppendLine();
                expressionPrinter.Append("EXCEPT");
                if (!IsDistinct)
                {
                    expressionPrinter.AppendLine(" ALL");
                }

                expressionPrinter.Visit(Source2);
            }

            expressionPrinter.AppendLine()
                .AppendLine($") AS {Alias}");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ExceptExpression exceptExpression
                    && Equals(exceptExpression));

        private bool Equals(ExceptExpression exceptExpression)
            => base.Equals(exceptExpression);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), GetType());
    }
}
