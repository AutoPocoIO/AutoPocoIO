﻿using AutoPocoIO.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly WeakEntityExpandingExpressionVisitor _weakEntityExpandingExpressionVisitor;
        private readonly bool _subquery;

        public RelationalQueryableMethodTranslatingExpressionVisitor(QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
                                                                     RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
                                                                     IModel model) : base(dependencies, relationalDependencies, model)
        {
            _model = model;
            _sqlTranslator = relationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(model, this);
            _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
            _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory as ISqlExpressionFactoryWithCrossDb;
            _weakEntityExpandingExpressionVisitor = new WeakEntityExpandingExpressionVisitor(_sqlTranslator, _sqlExpressionFactory);
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

        protected override ShapedQueryExpression TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate);
            if (translation == null)
            {
                return null;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ApplyPredicate(_sqlExpressionFactory.Not(translation));
            selectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression>());
            if (selectExpression.Limit == null
                && selectExpression.Offset == null)
            {
                selectExpression.ClearOrdering();
            }

            translation = _sqlExpressionFactory.Exists(selectExpression, true);
            source.QueryExpression = _sqlExpressionFactory.Select(translation);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool));

            return source;
        }

        protected override ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
                if (source == null)
                {
                    return null;
                }
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression>());
            if (selectExpression.Limit == null
                && selectExpression.Offset == null)
            {
                selectExpression.ClearOrdering();
            }

            var translation = _sqlExpressionFactory.Exists(selectExpression, false);
            source.QueryExpression = _sqlExpressionFactory.Select(translation);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool));

            return source;
        }

        protected override ShapedQueryExpression TranslateAverage(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var newSelector = selector == null
                || selector.Body == selector.Parameters[0]
                    ? selectExpression.GetMappedProjection(new ProjectionMember())
                    : RemapLambdaBody(source, selector);

            var projection = _sqlTranslator.TranslateAverage(newSelector);
            return projection != null
                ? AggregateResultShaper(source, projection, throwOnNullResult: true, resultType)
                : null;
        }

        protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: false);

            return source1;
        }

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(item);
            if (translation == null)
            {
                return null;
            }

            if (selectExpression.Limit == null
                && selectExpression.Offset == null)
            {
                selectExpression.ClearOrdering();
            }

            selectExpression.ApplyProjection();
            translation = _sqlExpressionFactory.In(translation, selectExpression, false);
            source.QueryExpression = _sqlExpressionFactory.Select(translation);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool));

            return source;
        }

        protected override ShapedQueryExpression TranslateCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
                if (source == null)
                {
                    return null;
                }
            }

            var translation = _sqlTranslator.TranslateCount();
            if (translation == null)
            {
                return null;
            }

            var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

            selectExpression.ClearOrdering();
            selectExpression.ReplaceProjectionMapping(projectionMapping);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(int));

            return source;
        }

        protected override ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue)
        {
            if (defaultValue == null)
            {
                ((SelectExpression)source.QueryExpression).ApplyDefaultIfEmpty(_sqlExpressionFactory);
                source.ShaperExpression = MarkShaperNullable(source.ShaperExpression);

                return source;
            }

            return null;
        }

        protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
        {
            ((SelectExpression)source.QueryExpression).ApplyDistinct();

            return source;
        }

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            ((SelectExpression)source1.QueryExpression).ApplyExcept((SelectExpression)source2.QueryExpression, distinct: true);
            return source1;
        }

        protected override ShapedQueryExpression TranslateFirstOrDefault(
            ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
                if (source == null)
                {
                    return null;
                }
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1)));

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }

        protected override ShapedQueryExpression TranslateGroupBy(
           ShapedQueryExpression source,
           LambdaExpression keySelector,
           LambdaExpression elementSelector,
           LambdaExpression resultSelector)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var remappedKeySelector = RemapLambdaBody(source, keySelector);

            var translatedKey = TranslateGroupingKey(remappedKeySelector);
            if (translatedKey != null)
            {
                if (elementSelector != null)
                {
                    source = TranslateSelect(source, elementSelector);
                }

                selectExpression.ApplyGrouping(translatedKey);
                source.ShaperExpression = new GroupByShaperExpression(translatedKey, source.ShaperExpression);

                if (resultSelector == null)
                {
                    return source;
                }

                var original1 = resultSelector.Parameters[0];
                var original2 = resultSelector.Parameters[1];

                //Switched to dictionary 
                var replacements = new Dictionary<Expression, Expression>
                {
                    { original1, translatedKey},
                    {original2,  source.ShaperExpression}
                };

                var newResultSelectorBody = new ReplacingExpressionVisitor(replacements)
                    .Visit(resultSelector.Body);

                newResultSelectorBody = ExpandWeakEntities(selectExpression, newResultSelectorBody);

                source.ShaperExpression = _projectionBindingExpressionVisitor.Translate(selectExpression, newResultSelectorBody);

                return source;
            }

            return null;
        }

        private Expression TranslateGroupingKey(Expression expression)
        {
            switch (expression)
            {
                case NewExpression newExpression:
                    // For .NET Framework only. If ctor is null that means the type is struct and has no ctor args.
                    if (newExpression.Constructor == null)
                    {
                        return newExpression;
                    }

                    if (newExpression.Arguments.Count == 0)
                    {
                        return newExpression;
                    }

                    var newArguments = new Expression[newExpression.Arguments.Count];
                    for (var i = 0; i < newArguments.Length; i++)
                    {
                        newArguments[i] = TranslateGroupingKey(newExpression.Arguments[i]);
                        if (newArguments[i] == null)
                        {
                            return null;
                        }
                    }

                    return newExpression.Update(newArguments);

                case MemberInitExpression memberInitExpression:
                    var updatedNewExpression = (NewExpression)TranslateGroupingKey(memberInitExpression.NewExpression);
                    if (updatedNewExpression == null)
                    {
                        return null;
                    }

                    var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
                    for (var i = 0; i < newBindings.Length; i++)
                    {
                        var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                        var visitedExpression = TranslateGroupingKey(memberAssignment.Expression);
                        if (visitedExpression == null)
                        {
                            return null;
                        }

                        newBindings[i] = memberAssignment.Update(visitedExpression);
                    }

                    return memberInitExpression.Update(updatedNewExpression, newBindings);

                default:
                    var translation = _sqlTranslator.Translate(expression);
                    if (translation == null)
                    {
                        return null;
                    }

                    return translation.Type == expression.Type
                        ? (Expression)translation
                        : Expression.Convert(translation, expression.Type);
            }
        }

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            ((SelectExpression)source1.QueryExpression).ApplyIntersect((SelectExpression)source2.QueryExpression, distinct: true);
            return source1;
        }

        protected override ShapedQueryExpression TranslateJoin(
           ShapedQueryExpression outer,
           ShapedQueryExpression inner,
           LambdaExpression outerKeySelector,
           LambdaExpression innerKeySelector,
           LambdaExpression resultSelector)
        {
            var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
            if (joinPredicate != null)
            {
                var transparentIdentifierType = TransparentIdentifierFactory.Create(
                    resultSelector.Parameters[0].Type,
                    resultSelector.Parameters[1].Type);

                ((SelectExpression)outer.QueryExpression).AddInnerJoin(
                    (SelectExpression)inner.QueryExpression, joinPredicate, transparentIdentifierType);

                return TranslateResultSelectorForJoin(
                    outer,
                    resultSelector,
                    inner.ShaperExpression,
                    transparentIdentifierType);
            }

            return null;
        }

        protected override ShapedQueryExpression TranslateLeftJoin(
            ShapedQueryExpression outer,
            ShapedQueryExpression inner,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
            if (joinPredicate != null)
            {
                var transparentIdentifierType = TransparentIdentifierFactory.Create(
                    resultSelector.Parameters[0].Type,
                    resultSelector.Parameters[1].Type);

                ((SelectExpression)outer.QueryExpression).AddLeftJoin(
                    (SelectExpression)inner.QueryExpression, joinPredicate, transparentIdentifierType);

                return TranslateResultSelectorForJoin(
                    outer,
                    resultSelector,
                    MarkShaperNullable(inner.ShaperExpression),
                    transparentIdentifierType);
            }

            return null;
        }

        private SqlExpression CreateJoinPredicate(
          ShapedQueryExpression outer,
          LambdaExpression outerKeySelector,
          ShapedQueryExpression inner,
          LambdaExpression innerKeySelector)
        {
            var outerKey = RemapLambdaBody(outer, outerKeySelector);
            var innerKey = RemapLambdaBody(inner, innerKeySelector);

            if (outerKey is NewExpression outerNew
                && outerNew.Type != typeof(AnonymousObject))
            {
                var innerNew = (NewExpression)innerKey;

                SqlExpression result = null;
                for (var i = 0; i < outerNew.Arguments.Count; i++)
                {
                    var joinPredicate = CreateJoinPredicate(outerNew.Arguments[i], innerNew.Arguments[i]);
                    result = result == null
                        ? joinPredicate
                        : _sqlExpressionFactory.AndAlso(result, joinPredicate);
                }

                return result;
            }

            return CreateJoinPredicate(outerKey, innerKey);
        }

        private SqlExpression CreateJoinPredicate(Expression outerKey, Expression innerKey)
           => TranslateExpression(Expression.Equal(outerKey, innerKey));

        protected override ShapedQueryExpression TranslateLastOrDefault(
           ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.Orderings.Count == 0)
            {
                return null;
            }

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
                if (source == null)
                {
                    return null;
                }
            }

            selectExpression.ReverseOrderings();
            selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1)));

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }

        protected override ShapedQueryExpression TranslateLongCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
                if (source == null)
                {
                    return null;
                }
            }

            var translation = _sqlTranslator.TranslateLongCount();
            if (translation == null)
            {
                return null;
            }

            var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

            selectExpression.ClearOrdering();
            selectExpression.ReplaceProjectionMapping(projectionMapping);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(long));

            return source;
        }

        protected override ShapedQueryExpression TranslateMax(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var newSelector = selector == null
                || selector.Body == selector.Parameters[0]
                    ? selectExpression.GetMappedProjection(new ProjectionMember())
                    : RemapLambdaBody(source, selector);

            var projection = _sqlTranslator.TranslateMax(newSelector);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

        protected override ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var newSelector = selector == null
                || selector.Body == selector.Parameters[0]
                    ? selectExpression.GetMappedProjection(new ProjectionMember())
                    : RemapLambdaBody(source, selector);

            var projection = _sqlTranslator.TranslateMin(newSelector);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

        protected override ShapedQueryExpression TranslateOfType(ShapedQueryExpression source, Type resultType)
        {
            if (source.ShaperExpression is EntityShaperExpression entityShaperExpression)
            {
                var entityType = entityShaperExpression.EntityType;
                if (entityType.ClrType == resultType)
                {
                    return source;
                }

                var baseType = entityType.GetAllBaseTypes().SingleOrDefault(et => et.ClrType == resultType);
                if (baseType != null)
                {
                    source.ShaperExpression = entityShaperExpression.WithEntityType(baseType);

                    return source;
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == resultType);
                if (derivedType != null)
                {
                    var selectExpression = (SelectExpression)source.QueryExpression;
                    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                    var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                    var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetMappedProjection(
                        projectionBindingExpression.ProjectionMember);
                    var discriminatorColumn = entityProjectionExpression.BindProperty(entityType.GetDiscriminatorProperty());

                    var predicate = concreteEntityTypes.Count == 1
                        ? _sqlExpressionFactory.Equal(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                        : (SqlExpression)_sqlExpressionFactory.In(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue())),
                            negated: false);

                    selectExpression.ApplyPredicate(predicate);

                    var projectionMember = projectionBindingExpression.ProjectionMember;

                    Debug.Assert(
                        new ProjectionMember().Equals(projectionMember),
                        "Invalid ProjectionMember when processing OfType");

                    var entityProjection = (EntityProjectionExpression)selectExpression.GetMappedProjection(projectionMember);

                    selectExpression.ReplaceProjectionMapping(
                        new Dictionary<ProjectionMember, Expression>
                        {
                            { projectionMember, entityProjection.UpdateEntityType(derivedType) }
                        });

                    source.ShaperExpression = entityShaperExpression.WithEntityType(derivedType);

                    return source;
                }

                // If the resultType is not part of hierarchy then we don't know how to materialize.
            }

            return null;
        }

        protected override ShapedQueryExpression TranslateOrderBy(
            ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var translation = TranslateLambdaExpression(source, keySelector);
            if (translation == null)
            {
                return null;
            }

            ((SelectExpression)source.QueryExpression).ApplyOrdering(new OrderingExpression(translation, ascending));

            return source;
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
            source.ShaperExpression = _projectionBindingExpressionVisitor.Translate(selectExpression, newSelectorBody);

            return source;
        }

        protected override ShapedQueryExpression TranslateSelectMany(
           ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        {
            var (newCollectionSelector, correlated, defaultIfEmpty)
                = new CorrelationFindingExpressionVisitor().IsCorrelated(collectionSelector);
            if (correlated)
            {
                var collectionSelectorBody = RemapLambdaBody(source, newCollectionSelector);
                if (Visit(collectionSelectorBody) is ShapedQueryExpression inner)
                {
                    var transparentIdentifierType = TransparentIdentifierFactory.Create(
                        resultSelector.Parameters[0].Type,
                        resultSelector.Parameters[1].Type);

                    var innerShaperExpression = inner.ShaperExpression;
                    if (defaultIfEmpty)
                    {
                        ((SelectExpression)source.QueryExpression).AddOuterApply(
                            (SelectExpression)inner.QueryExpression, transparentIdentifierType);
                        innerShaperExpression = MarkShaperNullable(innerShaperExpression);
                    }
                    else
                    {
                        ((SelectExpression)source.QueryExpression).AddCrossApply(
                            (SelectExpression)inner.QueryExpression, transparentIdentifierType);
                    }

                    return TranslateResultSelectorForJoin(
                        source,
                        resultSelector,
                        innerShaperExpression,
                        transparentIdentifierType);
                }
            }
            else
            {
                if (Visit(newCollectionSelector.Body) is ShapedQueryExpression inner)
                {
                    if (defaultIfEmpty)
                    {
                        inner = TranslateDefaultIfEmpty(inner, null);
                        if (inner == null)
                        {
                            return null;
                        }
                    }

                    var transparentIdentifierType = TransparentIdentifierFactory.Create(
                        resultSelector.Parameters[0].Type,
                        resultSelector.Parameters[1].Type);

                    ((SelectExpression)source.QueryExpression).AddCrossJoin(
                        (SelectExpression)inner.QueryExpression, transparentIdentifierType);

                    return TranslateResultSelectorForJoin(
                        source,
                        resultSelector,
                        inner.ShaperExpression,
                        transparentIdentifierType);
                }
            }

            return null;
        }

        private class CorrelationFindingExpressionVisitor : ExpressionVisitor
        {
            private ParameterExpression _outerParameter;
            private bool _correlated;
            private bool _defaultIfEmpty;

            public (LambdaExpression, bool, bool) IsCorrelated(LambdaExpression lambdaExpression)
            {
                Debug.Assert(lambdaExpression.Parameters.Count == 1, "Multiparameter lambda passed to CorrelationFindingExpressionVisitor");

                _correlated = false;
                _defaultIfEmpty = false;
                _outerParameter = lambdaExpression.Parameters[0];

                var result = Visit(lambdaExpression.Body);

                return (Expression.Lambda(result, _outerParameter), _correlated, _defaultIfEmpty);
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                if (parameterExpression == _outerParameter)
                {
                    _correlated = true;
                }

                return base.VisitParameter(parameterExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
                {
                    _defaultIfEmpty = true;
                    return Visit(methodCallExpression.Arguments[0]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
        {
            var innerParameter = Expression.Parameter(selector.ReturnType.TryGetSequenceType(), "i");
            var resultSelector = Expression.Lambda(
                innerParameter, Expression.Parameter(source.Type.TryGetSequenceType()), innerParameter);

            return TranslateSelectMany(source, selector, resultSelector);
        }

        protected override ShapedQueryExpression TranslateSingleOrDefault(
            ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
                if (source == null)
                {
                    return null;
                }
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(_subquery ? 1 : 2)));

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }

        protected override ShapedQueryExpression TranslateSkip(ShapedQueryExpression source, Expression count)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(count);
            if (translation == null)
            {
                return null;
            }

            selectExpression.ApplyOffset(translation);

            return source;
        }

        protected override ShapedQueryExpression TranslateSum(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();
            var newSelector = selector == null
                || selector.Body == selector.Parameters[0]
                    ? selectExpression.GetMappedProjection(new ProjectionMember())
                    : RemapLambdaBody(source, selector);

            var projection = _sqlTranslator.TranslateSum(newSelector);
            return projection != null
                ? AggregateResultShaper(source, projection, throwOnNullResult: false, resultType)
                : null;
        }

        protected override ShapedQueryExpression TranslateTake(ShapedQueryExpression source, Expression count)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(count);
            if (translation == null)
            {
                return null;
            }

            selectExpression.ApplyLimit(translation);

            return source;
        }

        protected override ShapedQueryExpression TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var translation = TranslateLambdaExpression(source, keySelector);
            if (translation == null)
            {
                return null;
            }

           ((SelectExpression)source.QueryExpression).AppendOrdering(new OrderingExpression(translation, ascending));

            return source;
        }

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: true);
            return source1;
        }

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate);
            if (translation == null)
            {
                return null;
            }

            ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

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

        private SqlExpression TranslateExpression(Expression expression) => _sqlTranslator.Translate(expression);

        private SqlExpression TranslateLambdaExpression(
           ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
           => TranslateExpression(RemapLambdaBody(shapedQueryExpression, lambdaExpression));

        private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

            return ExpandWeakEntities((SelectExpression)shapedQueryExpression.QueryExpression, lambdaBody);
        }

        internal Expression ExpandWeakEntities(SelectExpression selectExpression, Expression lambdaBody)
           => _weakEntityExpandingExpressionVisitor.Expand(selectExpression, lambdaBody);

        private class WeakEntityExpandingExpressionVisitor : ExpressionVisitor
        {
            private SelectExpression _selectExpression;
            private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
            private readonly ISqlExpressionFactoryWithCrossDb _sqlExpressionFactory;

            public WeakEntityExpandingExpressionVisitor(
                RelationalSqlTranslatingExpressionVisitor sqlTranslator,
                ISqlExpressionFactoryWithCrossDb sqlExpressionFactory)
            {
                _sqlTranslator = sqlTranslator;
                _sqlExpressionFactory = sqlExpressionFactory;
            }

            public virtual Expression Expand(SelectExpression selectExpression, Expression lambdaBody)
            {
                _selectExpression = selectExpression;

                return Visit(lambdaBody);
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                var innerExpression = Visit(memberExpression.Expression);

                return TryExpand(innerExpression, MemberIdentity.Create(memberExpression.Member))
                    ?? memberExpression.Update(innerExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName))
                {
                    source = Visit(source);

                    return TryExpand(source, MemberIdentity.Create(navigationName))
                        ?? methodCallExpression.Update(null, new[] { source, methodCallExpression.Arguments[1] });
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
                => extensionExpression is EntityShaperExpression
                    ? extensionExpression
                    : base.VisitExtension(extensionExpression);

            private Expression TryExpand(Expression source, MemberIdentity member)
            {
                source = source.UnwrapTypeConversion(out var convertedType);
                if (!(source is EntityShaperExpression entityShaperExpression))
                {
                    return null;
                }

                var entityType = entityShaperExpression.EntityType;
                if (convertedType != null)
                {
                    entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                        .FirstOrDefault(et => et.ClrType == convertedType);

                    if (entityType == null)
                    {
                        return null;
                    }
                }

                var navigation = member.MemberInfo != null
                    ? entityType.FindNavigation(member.MemberInfo)
                    : entityType.FindNavigation(member.Name);

                if (navigation == null)
                {
                    return null;
                }

                var targetEntityType = navigation.GetTargetType();
                if (targetEntityType == null
                    || (!targetEntityType.HasDefiningNavigation()
                        && !targetEntityType.IsOwned()))
                {
                    return null;
                }

                var foreignKey = navigation.ForeignKey;
                if (navigation.IsCollection())
                {
                    var innerShapedQuery = CreateShapedQueryExpression(
                        targetEntityType, _sqlExpressionFactory.SelectWithCrossDb(targetEntityType));

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var innerSequenceType = innerShapedQuery.Type.TryGetSequenceType();
                    var correlationPredicateParameter = Expression.Parameter(innerSequenceType);

                    var outerKey = entityShaperExpression.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
                        makeNullable);
                    var innerKey = correlationPredicateParameter.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.PrincipalKey.Properties
                            : foreignKey.Properties,
                        makeNullable);

                    var outerKeyFirstProperty = outerKey is NewExpression newExpression
                        ? ((UnaryExpression)((NewArrayExpression)newExpression.Arguments[0]).Expressions[0]).Operand
                        : outerKey;

                    var predicate = outerKeyFirstProperty.Type.IsNullableType()
                        ? Expression.AndAlso(
                            Expression.NotEqual(outerKeyFirstProperty, Expression.Constant(null, outerKeyFirstProperty.Type)),
                            Expression.Equal(outerKey, innerKey))
                        : Expression.Equal(outerKey, innerKey);

                    var correlationPredicate = Expression.Lambda(predicate, correlationPredicateParameter);

                    return Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(innerSequenceType),
                        innerShapedQuery,
                        Expression.Quote(correlationPredicate));
                }

                var entityProjectionExpression = (EntityProjectionExpression)
                    (entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression
                        ? _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : entityShaperExpression.ValueBufferExpression);

                var innerShaper = entityProjectionExpression.BindNavigation(navigation);
                if (innerShaper == null)
                {
                    var innerSelectExpression = _sqlExpressionFactory.SelectWithCrossDb(targetEntityType);
                    var innerShapedQuery = CreateShapedQueryExpression(targetEntityType, innerSelectExpression);

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var outerKey = entityShaperExpression.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
                        makeNullable);
                    var innerKey = innerShapedQuery.ShaperExpression.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.PrincipalKey.Properties
                            : foreignKey.Properties,
                        makeNullable);

                    var joinPredicate = _sqlTranslator.Translate(Expression.Equal(outerKey, innerKey));
                    _selectExpression.AddLeftJoin(innerSelectExpression, joinPredicate, null);
                    var leftJoinTable = ((LeftJoinExpression)_selectExpression.Tables.Last()).Table;
                    innerShaper = new EntityShaperExpression(
                        targetEntityType,
                        new EntityProjectionExpression(targetEntityType, leftJoinTable, true),
                        true);
                    entityProjectionExpression.AddNavigationBinding(navigation, innerShaper);
                }

                return innerShaper;
            }
        }

        private ShapedQueryExpression AggregateResultShaper(
           ShapedQueryExpression source, Expression projection, bool throwOnNullResult, Type resultType)
        {
            if (projection == null)
            {
                return null;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ReplaceProjectionMapping(
                new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), projection } });

            selectExpression.ClearOrdering();

            var nullableResultType = resultType.MakeNullable();
            Expression shaper = new ProjectionBindingExpression(
                source.QueryExpression, new ProjectionMember(), throwOnNullResult ? nullableResultType : projection.Type);

            if (throwOnNullResult)
            {
                var resultVariable = Expression.Variable(nullableResultType, "result");
                var returnValueForNull = resultType.IsNullableType()
                    ? (Expression)Expression.Constant(null, resultType)
                    : Expression.Throw(
                        Expression.New(
                            typeof(InvalidOperationException).GetConstructors()
                                .Single(ci => ci.GetParameters().Length == 1),
                            Expression.Constant(CoreStrings.NoElements)),
                        resultType);

                shaper = Expression.Block(
                    new[] { resultVariable },
                    Expression.Assign(resultVariable, shaper),
                    Expression.Condition(
                        Expression.Equal(resultVariable, Expression.Default(nullableResultType)),
                        returnValueForNull,
                        resultType != resultVariable.Type
                            ? Expression.Convert(resultVariable, resultType)
                            : (Expression)resultVariable));
            }
            else if (resultType != shaper.Type)
            {
                shaper = Expression.Convert(shaper, resultType);
            }

            source.ShaperExpression = shaper;

            return source;
        }
    }
}
