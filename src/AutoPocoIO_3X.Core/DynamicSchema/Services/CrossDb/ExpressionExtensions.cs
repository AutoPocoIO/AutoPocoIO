using System;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal static class ExpressionExtensions
    {
        public static LambdaExpression UnwrapLambdaFromQuote(this Expression expression)
            => (LambdaExpression)(expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
                ? unary.Operand
                : expression);

        public static Expression UnwrapTypeConversion(this Expression expression, out Type convertedType)
        {
            convertedType = null;
            while (expression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                expression = unaryExpression.Operand;
                if (unaryExpression.Type != typeof(object) // Ignore object conversion
                    && !unaryExpression.Type.IsAssignableFrom(expression.Type)) // Ignore casting to base type/interface
                {
                    convertedType = unaryExpression.Type;
                }
            }

            return expression;
        }
    }
}
