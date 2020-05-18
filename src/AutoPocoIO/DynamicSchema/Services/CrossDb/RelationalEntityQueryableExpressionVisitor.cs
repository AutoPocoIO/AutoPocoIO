using AutoPocoIO.CustomAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class RelationalEntityQueryableExpressionVisitor : Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.RelationalEntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IQuerySource _querySource;
        private new RelationalQueryModelVisitor QueryModelVisitor => (RelationalQueryModelVisitor)base.QueryModelVisitor;
        private static readonly MethodInfo _createEntityShaperMethodInfo
           = typeof(RelationalEntityQueryableExpressionVisitor).GetTypeInfo()
               .GetDeclaredMethod(nameof(CreateEntityShaper));
        public RelationalEntityQueryableExpressionVisitor(RelationalEntityQueryableExpressionVisitorDependencies dependencies
            , RelationalQueryModelVisitor queryModelVisitor, IQuerySource querySource)
            : base(dependencies, queryModelVisitor, querySource)
        {
            _model = dependencies.Model;
            _selectExpressionFactory = dependencies.SelectExpressionFactory;
            _shaperCommandContextFactory = dependencies.ShaperCommandContextFactory;
            _materializerFactory = dependencies.MaterializerFactory;
            _querySource = querySource;
        }

        protected override Expression VisitEntityQueryable(Type elementType)
        {

            var relationalQueryCompilationContext = QueryModelVisitor.QueryCompilationContext;

            var entityType = relationalQueryCompilationContext.FindEntityType(_querySource)
                             ?? _model.FindEntityType(elementType);

            var selectExpression = _selectExpressionFactory.Create(relationalQueryCompilationContext);

            QueryModelVisitor.AddQuery(_querySource, selectExpression);

            var tableName = entityType.Relational().TableName;
            var databaseName = elementType.CustomAttributes
                                            .FirstOrDefault(c => c.AttributeType == typeof(DatabaseNameAttribute))
                                            ?.ConstructorArguments[0]
                                            .Value
                                            .ToString();

            var tableAlias
                = relationalQueryCompilationContext.CreateUniqueTableAlias(
                    _querySource.HasGeneratedItemName()
                        ? tableName[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant()
                        : (_querySource as GroupJoinClause)?.JoinClause.ItemName
                          ?? _querySource.ItemName);

            var fromSqlAnnotation
                = relationalQueryCompilationContext
                    .QueryAnnotations
                    .OfType<FromSqlResultOperator>()
                    .LastOrDefault(a => a.QuerySource == _querySource);

            Func<IQuerySqlGenerator> querySqlGeneratorFunc = selectExpression.CreateDefaultQuerySqlGenerator;

            if (fromSqlAnnotation == null)
            {
                selectExpression.AddTable(
                    new TableExpression(
                        databaseName,
                        tableName,
                        entityType.Relational().Schema,
                        tableAlias,
                        _querySource));
            }
            else
            {
                selectExpression.AddTable(
                    new FromSqlExpression(
                        fromSqlAnnotation.Sql,
                        fromSqlAnnotation.Arguments,
                        tableAlias,
                        _querySource));

                var trimmedSql = fromSqlAnnotation.Sql.TrimStart('\r', '\n', '\t', ' ');

                var useQueryComposition
                    = trimmedSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase)
                      || trimmedSql.StartsWith("SELECT" + Environment.NewLine, StringComparison.OrdinalIgnoreCase)
                      || trimmedSql.StartsWith("SELECT\t", StringComparison.OrdinalIgnoreCase);

                var requiresClientEval = !useQueryComposition;

                if (!useQueryComposition
                    && relationalQueryCompilationContext.IsIncludeQuery)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.StoredProcedureIncludeNotSupported);
                }

                if (useQueryComposition
                    && fromSqlAnnotation.QueryModel.IsIdentityQuery()
                    && !fromSqlAnnotation.QueryModel.ResultOperators.Any()
                    && !relationalQueryCompilationContext.IsIncludeQuery
                    && entityType.BaseType == null
                    && !entityType.GetDerivedTypes().Any())
                {
                    useQueryComposition = false;
                }

                if (!useQueryComposition)
                {
                    QueryModelVisitor.RequiresClientEval = requiresClientEval;

                    querySqlGeneratorFunc = ()
                        => selectExpression.CreateFromSqlQuerySqlGenerator(
                            fromSqlAnnotation.Sql,
                            fromSqlAnnotation.Arguments);
                }
            }

            var shaper = CreateShaper(elementType, entityType, selectExpression);

            DiscriminateProjectionQuery(entityType, selectExpression, _querySource);

            return Expression.Call(
                QueryModelVisitor.QueryCompilationContext.QueryMethodProvider
                    .ShapedQueryMethod
                    .MakeGenericMethod(shaper.Type),
                EntityQueryModelVisitor.QueryContextParameter,
                Expression.Constant(_shaperCommandContextFactory.Create(querySqlGeneratorFunc)),
                Expression.Constant(shaper));
        }

        private Shaper CreateShaper(Type elementType, IEntityType entityType, Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression selectExpression)
        {
            Shaper shaper;

            if (QueryModelVisitor.QueryCompilationContext
                    .QuerySourceRequiresMaterialization(_querySource)
                || QueryModelVisitor.RequiresClientEval)
            {
                var materializerExpression
                    = _materializerFactory
                        .CreateMaterializer(
                            entityType,
                            selectExpression,
                            (p, se) =>
                                se.AddToProjection(
                                    p,
                                    _querySource),
                            out var typeIndexMap);

                var materializer = materializerExpression.Compile();

                shaper
                    = (Shaper)_createEntityShaperMethodInfo.MakeGenericMethod(elementType)
                        .Invoke(
                            obj: null,
                            parameters: new object[]
                            {
                                _querySource,
                                QueryModelVisitor.QueryCompilationContext.IsTrackingQuery
                                && !entityType.IsQueryType,
                                entityType.FindPrimaryKey(),
                                materializer,
                                materializerExpression,
                                typeIndexMap,
                                QueryModelVisitor.QueryCompilationContext.IsQueryBufferRequired
                                && !entityType.IsQueryType
                            });
            }
            else
            {
                shaper = new ValueBufferShaper(_querySource);
            }

            return shaper;
        }

        private static IShaper<TEntity> CreateEntityShaper<TEntity>(
           IQuerySource querySource,
           bool trackingQuery,
           IKey key,
           Func<MaterializationContext, object> materializer,
           Expression materializerExpression,
           Dictionary<Type, int[]> typeIndexMap,
           bool useQueryBuffer)
           where TEntity : class
           => !useQueryBuffer
               ? (IShaper<TEntity>)new UnbufferedEntityShaper<TEntity>(
                   querySource,
                   trackingQuery,
                   key,
                   materializer,
                   materializerExpression)
               : new BufferedEntityShaper<TEntity>(
                   querySource,
                   trackingQuery,
                   key,
                   materializer,
                   typeIndexMap);

        private static void FindPaths(
    IEntityType entityType, ICollection<IEntityType> sharedTypes,
    Stack<IEntityType> currentPath, ICollection<List<IEntityType>> result)
        {
            var identifyingFks = entityType.FindForeignKeys(entityType.FindPrimaryKey().Properties)
                .Where(
                    fk => fk.PrincipalKey.IsPrimaryKey()
                          && fk.PrincipalEntityType != entityType
                          && sharedTypes.Contains(fk.PrincipalEntityType))
                .ToList();

            if (identifyingFks.Count == 0)
            {
                result.Add(new List<IEntityType>(currentPath));
                return;
            }

            foreach (var fk in identifyingFks)
            {
                currentPath.Push(fk.PrincipalEntityType);
                FindPaths(fk.PrincipalEntityType.RootType(), sharedTypes, currentPath, result);
                currentPath.Pop();
            }
        }

        private static Expression GenerateDiscriminatorExpression(
          IEntityType entityType, Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression selectExpression, IQuerySource querySource)
        {
            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToList();

            if (concreteEntityTypes.Count == 1
                && concreteEntityTypes[0].RootType() == concreteEntityTypes[0])
            {
                return null;
            }

            var discriminatorColumn
                = selectExpression.BindProperty(
                    concreteEntityTypes[0].Relational().DiscriminatorProperty,
                    querySource);

            var firstDiscriminatorValue
                = Expression.Constant(
                    concreteEntityTypes[0].Relational().DiscriminatorValue,
                    discriminatorColumn.Type);

            var discriminatorPredicate
                = Expression.Equal(discriminatorColumn, firstDiscriminatorValue);

            if (concreteEntityTypes.Count > 1)
            {
                discriminatorPredicate
                    = concreteEntityTypes
                        .Skip(1)
                        .Select(
                            concreteEntityType
                                => Expression.Constant(
                                    concreteEntityType.Relational().DiscriminatorValue,
                                    discriminatorColumn.Type))
                        .Aggregate(
                            discriminatorPredicate, (current, discriminatorValue) =>
                                Expression.OrElse(
                                    Expression.Equal(discriminatorColumn, discriminatorValue),
                                    current));
            }

            return discriminatorPredicate;
        }

        private void DiscriminateProjectionQuery(
           IEntityType entityType, Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpression selectExpression, IQuerySource querySource)
        {
            Expression discriminatorPredicate;

            if (entityType.IsQueryType)
            {
                discriminatorPredicate = GenerateDiscriminatorExpression(entityType, selectExpression, querySource);
            }
            else
            {
                var sharedTypes = new HashSet<IEntityType>(
                    _model.GetEntityTypes()
                        .Where(e => !e.IsQueryType)
                        .Where(
                            et => et.Relational().TableName == entityType.Relational().TableName
                                  && et.Relational().Schema == entityType.Relational().Schema));

                var currentPath = new Stack<IEntityType>();
                currentPath.Push(entityType);

                var allPaths = new List<List<IEntityType>>();
                FindPaths(entityType.RootType(), sharedTypes, currentPath, allPaths);

                discriminatorPredicate = allPaths
                    .Select(
                        p => p.Select(
                                et => GenerateDiscriminatorExpression(et, selectExpression, querySource))
                            .Aggregate(
                                (Expression)null,
                                (result, current) => result != null
                                    ? current != null
                                        ? Expression.AndAlso(result, current)
                                        : result
                                    : current))
                    .Aggregate(
                        (Expression)null,
                        (result, current) => result != null
                            ? current != null
                                ? Expression.OrElse(result, current)
                                : result
                            : current);
            }

            if (discriminatorPredicate != null)
            {
                selectExpression.Predicate = new DiscriminatorPredicateExpression(discriminatorPredicate, querySource);
            }
        }
    }
}
