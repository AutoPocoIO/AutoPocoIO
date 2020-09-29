using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{

    internal class RelationalQueryableMethodTranslatingExpressionVisitor : Microsoft.EntityFrameworkCore.Query.RelationalQueryableMethodTranslatingExpressionVisitor
    {
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly IModel _model;
        protected readonly ISqlExpressionFactoryWithCrossDb _sqlExpressionFactory;
        private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;

        public RelationalQueryableMethodTranslatingExpressionVisitor(QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
                                                                     RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
                                                                     IModel model) : base(dependencies, relationalDependencies, model)
        {
            _model = model;
            _sqlTranslator = relationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(model, this);
            _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
            _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory as ISqlExpressionFactoryWithCrossDb;
        }

        protected RelationalQueryableMethodTranslatingExpressionVisitor(RelationalQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor)
        {
            _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        }

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            var queryExpression = _sqlExpressionFactory.SelectWithCrossDb(entityType);

            return CreateShapedQueryExpression(entityType, queryExpression);
        }

        protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
        {
            if (selector.Body == selector.Parameters[0])
            {
                return source;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct)
            {
                selectExpression.PushdownIntoSubquery();
            }

            var newSelectorBody = ReplacingExpressionVisitor.Replace(
                selector.Parameters.Single(), source.ShaperExpression, selector.Body);
          //  source.ShaperExpression = _projectionBindingExpressionVisitor.Translate(selectExpression, newSelectorBody);

            return source;
        }

        private static ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, SelectExpression selectExpression)
       => new ShapedQueryExpression(
           selectExpression,
           new EntityShaperExpression(
               entityType,
               new ProjectionBindingExpression(
                   selectExpression,
                   new ProjectionMember(),
                   typeof(ValueBuffer)),
               false));
    }
}
