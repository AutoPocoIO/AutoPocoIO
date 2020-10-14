using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    /// <summary>
    /// Cross db override
    /// </summary>
    internal class RelationalProjectionBindingExpressionVisitor : Microsoft.EntityFrameworkCore.Query.Internal.RelationalProjectionBindingExpressionVisitor
    {
        private readonly RelationalQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;


        private SelectExpression _selectExpression;
        private bool _clientEval;


        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();
        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();

        public RelationalProjectionBindingExpressionVisitor(
            Microsoft.EntityFrameworkCore.Query.RelationalQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            RelationalSqlTranslatingExpressionVisitor sqlTranslatingExpressionVisitor)
            : base(queryableMethodTranslatingExpressionVisitor, sqlTranslatingExpressionVisitor)
        {
        }

        public virtual Expression Translate(SelectExpression selectExpression, Expression expression)
        {
            _selectExpression = selectExpression;
            _clientEval = false;

            _projectionMembers.Push(new ProjectionMember());

            var expandedExpression = _queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(_selectExpression, expression);
            var result = Visit(expandedExpression);

            if (result == null)
            {
                _clientEval = true;

                expandedExpression = _queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(_selectExpression, expression);
                result = Visit(expandedExpression);

                _projectionMapping.Clear();
            }

            _selectExpression.ReplaceProjectionMapping(_projectionMapping);
            _selectExpression = null;
            _projectionMembers.Clear();
            _projectionMapping.Clear();

            return result;
        }
    }
}
