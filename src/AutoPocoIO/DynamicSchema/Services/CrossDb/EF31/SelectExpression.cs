using AutoPocoIO.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class SelectExpression : Microsoft.EntityFrameworkCore.Query.SqlExpressions.TableExpressionBase
    {
        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
        private readonly List<ProjectionExpression> _projection = new List<ProjectionExpression>();
        private readonly List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<SqlExpression> _groupBy = new List<SqlExpression>();
        private readonly List<OrderingExpression> _orderings = new List<OrderingExpression>();
        private readonly List<SqlExpression> _identifier = new List<SqlExpression>();
        private readonly List<SqlExpression> _childIdentifiers = new List<SqlExpression>();
        private readonly List<SelectExpression> _pendingCollections = new List<SelectExpression>();


        internal SelectExpression(
            string alias,
            List<ProjectionExpression> projections,
            List<TableExpressionBase> tables,
            List<SqlExpression> groupBy,
            List<OrderingExpression> orderings)
            : base(alias)
        {
            _projection = projections;
            _tables = tables;
            _groupBy = groupBy;
            _orderings = orderings;
        }

        internal SelectExpression(IEntityType entityType) : base(null)
        {



            var tableExpression = new TableExpression(
                    entityType.GetDatabase(),
                    entityType.GetTableName(),
                    entityType.GetSchema(),
                    entityType.GetTableName().ToLower().Substring(0, 1));


            _tables.Add(tableExpression);
            var entityProjection = new EntityProjectionExpression(entityType, tableExpression, false);
            _projectionMapping[new ProjectionMember()] = entityProjection;

            if (entityType.FindPrimaryKey() != null)
            {
                foreach (var property in entityType.FindPrimaryKey().Properties)
                {
                    _identifier.Add(entityProjection.BindProperty(property));
                }
            }
        }

        public IReadOnlyList<ProjectionExpression> Projection => _projection;
        public IReadOnlyList<TableExpressionBase> Tables => _tables;
        public IReadOnlyList<SqlExpression> GroupBy => _groupBy;
        public IReadOnlyList<OrderingExpression> Orderings => _orderings;
        public ISet<string> Tags { get; private set; } = new HashSet<string>();
        public SqlExpression Predicate { get; private set; }
        public SqlExpression Having { get; private set; }
        public SqlExpression Limit { get; private set; }
        public SqlExpression Offset { get; private set; }
        public bool IsDistinct { get; private set; }

        public void ClearOrdering()
        {
            _orderings.Clear();
        }

        private enum SetOperationType
        {
            Except,
            Intersect,
            Union
        }

        private int AddToProjection(SqlExpression sqlExpression, string alias)
        {
            var existingIndex = _projection.FindIndex(pe => pe.Expression.Equals(sqlExpression));
            if (existingIndex != -1)
            {
                return existingIndex;
            }

            var baseAlias = alias ?? (sqlExpression as ColumnExpression)?.Name ?? (Alias != null ? "c" : null);
            var currentAlias = baseAlias ?? "";
            if (Alias != null
                && baseAlias != null)
            {
                var counter = 0;
                while (_projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
                {
                    currentAlias = $"{baseAlias}{counter++}";
                }
            }

            _projection.Add(new ProjectionExpression(sqlExpression, currentAlias));

            return _projection.Count - 1;
        }

        private bool ContainsTableReference(TableExpressionBase table)
           => Tables.Any(te => ReferenceEquals(te is JoinExpressionBase jeb ? jeb.Table : te, table));

        public int AddToProjection(SqlExpression sqlExpression)
          => AddToProjection(sqlExpression, null);

        public Expression GetMappedProjection(ProjectionMember projectionMember)
           => _projectionMapping[projectionMember];

        private static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
         => entityType.GetTypesInHierarchy().SelectMany(Microsoft.EntityFrameworkCore.EntityTypeExtensions.GetDeclaredProperties);

        public void AddInnerJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate, Type transparentIdentifierType)
          => AddJoin(JoinType.InnerJoin, innerSelectExpression, transparentIdentifierType, joinPredicate);

        public void ApplyPredicate(SqlExpression expression)
        {
            if (expression is SqlConstantExpression sqlConstant
                && (bool)sqlConstant.Value)
            {
                return;
            }

            if (Limit != null
                || Offset != null)
            {
                expression = new SqlRemappingVisitor(PushdownIntoSubquery(), (SelectExpression)Tables[0]).Remap(expression);
            }

            if (_groupBy.Count > 0)
            {
                Having = Having == null
                    ? expression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        Having,
                        expression,
                        typeof(bool),
                        expression.TypeMapping);
            }
            else
            {
                Predicate = Predicate == null
                    ? expression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        Predicate,
                        expression,
                        typeof(bool),
                        expression.TypeMapping);
            }
        }
        public void ApplyLimit(SqlExpression sqlExpression)
        {
            if (Limit != null)
            {
                PushdownIntoSubquery();
            }

            Limit = sqlExpression;
        }

        public void ApplyUnion(SelectExpression source2, bool distinct)
            => ApplySetOperation(SetOperationType.Union, source2, distinct);

        private void ApplySetOperation(SetOperationType setOperationType, SelectExpression select2, bool distinct)
        {
            // TODO: throw if there are pending collection joins
            // TODO: What happens when applying set operations on 2 queries with one of them being grouping

            var select1 = new SelectExpression(
                null, new List<ProjectionExpression>(), _tables.ToList(), _groupBy.ToList(), _orderings.ToList())
            {
                IsDistinct = IsDistinct,
                Predicate = Predicate,
                Having = Having,
                Offset = Offset,
                Limit = Limit
            };

            select1._projectionMapping = new Dictionary<ProjectionMember, Expression>(_projectionMapping);
            _projectionMapping.Clear();

            select1._identifier.AddRange(_identifier);
            _identifier.Clear();

            if (select1.Orderings.Count != 0
                || select1.Limit != null
                || select1.Offset != null)
            {
                select1.PushdownIntoSubquery();
                select1.ClearOrdering();
            }

            if (select2.Orderings.Count != 0
                || select2.Limit != null
                || select2.Offset != null)
            {
                select2.PushdownIntoSubquery();
                select2.ClearOrdering();
            }

            var setExpression = setOperationType switch
            {
                SetOperationType.Except => (SetOperationBase)new ExceptExpression("t", select1, select2, distinct),
                SetOperationType.Intersect => new IntersectExpression("t", select1, select2, distinct),
                SetOperationType.Union => new UnionExpression("t", select1, select2, distinct),
                _ => throw new InvalidOperationException($"Invalid {nameof(setOperationType)}: {setOperationType}")
            };

            if (_projection.Any()
                || select2._projection.Any())
            {
                throw new InvalidOperationException(
                    "Can't process set operations after client evaluation, consider moving the operation"
                    + " before the last Select() call (see issue #16243)");
            }

            if (select1._projectionMapping.Count != select2._projectionMapping.Count)
            {
                // Should not be possible after compiler checks
                throw new InvalidOperationException("Different projection mapping count in set operation");
            }

            foreach (var joinedMapping in select1._projectionMapping.Join(
                select2._projectionMapping,
                kv => kv.Key,
                kv => kv.Key,
                (kv1, kv2) => (kv1.Key, Value1: kv1.Value, Value2: kv2.Value)))
            {
                if (joinedMapping.Value1 is EntityProjectionExpression entityProjection1
                    && joinedMapping.Value2 is EntityProjectionExpression entityProjection2)
                {
                    HandleEntityMapping(joinedMapping.Key, select1, entityProjection1, select2, entityProjection2);
                    continue;
                }

                if (joinedMapping.Value1 is SqlExpression innerColumn1
                    && joinedMapping.Value2 is SqlExpression innerColumn2)
                {
                    // For now, make sure that both sides output the same store type, otherwise the query may fail.
                    // TODO: with #15586 we'll be able to also allow different store types which are implicitly convertible to one another.
                    if (innerColumn1.TypeMapping.StoreType != innerColumn2.TypeMapping.StoreType)
                    {
                        throw new InvalidOperationException("Set operations over different store types are currently unsupported");
                    }

                    var alias = GenerateUniqueAlias(
                        joinedMapping.Key.Last?.Name
                        ?? (innerColumn1 as ColumnExpression)?.Name
                        ?? "c");

                    var innerProjection1 = new ProjectionExpression(innerColumn1, alias);
                    var innerProjection2 = new ProjectionExpression(innerColumn2, alias);
                    select1._projection.Add(innerProjection1);
                    select2._projection.Add(innerProjection2);
                    var outerProjection = new ColumnExpression(innerProjection1, setExpression);

                    if (IsNullableProjection(innerProjection1)
                        || IsNullableProjection(innerProjection2))
                    {
                        outerProjection = outerProjection.MakeNullable();
                    }

                    _projectionMapping[joinedMapping.Key] = outerProjection;
                    continue;
                }

                throw new InvalidOperationException(
                    $"Non-matching or unknown projection mapping type in set operation ({joinedMapping.Value1.GetType().Name} and {joinedMapping.Value2.GetType().Name})");
            }

            Offset = null;
            Limit = null;
            IsDistinct = false;
            Predicate = null;
            Having = null;
            _groupBy.Clear();
            _orderings.Clear();
            _tables.Clear();
            _tables.Add(setExpression);

            void HandleEntityMapping(
                ProjectionMember projectionMember,
                SelectExpression select1, EntityProjectionExpression projection1,
                SelectExpression select2, EntityProjectionExpression projection2)
            {
                if (projection1.EntityType != projection2.EntityType)
                {
                    throw new InvalidOperationException(
                        "Set operations over different entity types are currently unsupported (see #16298)");
                }

                var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                foreach (var property in GetAllPropertiesInHierarchy(projection1.EntityType))
                {
                    propertyExpressions[property] = AddSetOperationColumnProjections(
                        select1, projection1.BindProperty(property),
                        select2, projection2.BindProperty(property));
                }

                _projectionMapping[projectionMember] = new EntityProjectionExpression(projection1.EntityType, propertyExpressions);
            }

            ColumnExpression AddSetOperationColumnProjections(
                SelectExpression select1, ColumnExpression column1,
                SelectExpression select2, ColumnExpression column2)
            {
                var alias = GenerateUniqueAlias(column1.Name);
                var innerProjection1 = new ProjectionExpression(column1, alias);
                var innerProjection2 = new ProjectionExpression(column2, alias);
                select1._projection.Add(innerProjection1);
                select2._projection.Add(innerProjection2);
                var outerProjection = new ColumnExpression(innerProjection1, setExpression);
                if (IsNullableProjection(innerProjection1)
                    || IsNullableProjection(innerProjection2))
                {
                    outerProjection = outerProjection.MakeNullable();
                }

                if (select1._identifier.Contains(column1))
                {
                    _identifier.Add(outerProjection);
                }

                return outerProjection;
            }

            string GenerateUniqueAlias(string baseAlias)
            {
                var currentAlias = baseAlias ?? "";
                var counter = 0;
                while (select1._projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
                {
                    currentAlias = $"{baseAlias}{counter++}";
                }

                return currentAlias;
            }

            static bool IsNullableProjection(ProjectionExpression projectionExpression)
                => projectionExpression.Expression switch
                {
                    ColumnExpression columnExpression => columnExpression.IsNullable,
                    SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
                    _ => true,
                };
        }

        private ColumnExpression GenerateOuterColumn(SqlExpression projection, string alias = null)
        {
            var index = AddToProjection(projection, alias);
            return new ColumnExpression(_projection[index], this);
        }

        public IDictionary<SqlExpression, ColumnExpression> PushdownIntoSubquery()
        {
            var subquery = new SelectExpression(
                "t", new List<ProjectionExpression>(), _tables.ToList(), _groupBy.ToList(), _orderings.ToList())
            {
                IsDistinct = IsDistinct,
                Predicate = Predicate,
                Having = Having,
                Offset = Offset,
                Limit = Limit
            };

            var projectionMap = new Dictionary<SqlExpression, ColumnExpression>();

            // Projections may be present if added by lifting SingleResult/Enumerable in projection through join
            if (_projection.Any())
            {
                var projections = _projection.Select(pe => pe.Expression).ToList();
                _projection.Clear();
                foreach (var projection in projections)
                {
                    var outerColumn = subquery.GenerateOuterColumn(projection);
                    AddToProjection(outerColumn);
                    projectionMap[projection] = outerColumn;
                }
            }

            foreach (var mapping in _projectionMapping.ToList())
            {
                // If projectionMapping's value is ConstantExpression then projection has already been applied
                // And captured in _projections above so we don't need to process this.
                if (mapping.Value is ConstantExpression)
                {
                    break;
                }

                if (mapping.Value is EntityProjectionExpression entityProjection)
                {
                    _projectionMapping[mapping.Key] = LiftEntityProjectionFromSubquery(entityProjection);
                }
                else
                {
                    var innerColumn = (SqlExpression)mapping.Value;
                    var outerColumn = subquery.GenerateOuterColumn(innerColumn);
                    projectionMap[innerColumn] = outerColumn;
                    _projectionMapping[mapping.Key] = outerColumn;
                }
            }

            var identifiers = _identifier.ToList();
            _identifier.Clear();
            // TODO: See issue#15873
            foreach (var identifier in identifiers)
            {
                if (projectionMap.TryGetValue(identifier, out var outerColumn))
                {
                    _identifier.Add(outerColumn);
                }
                else if (!IsDistinct
                    && GroupBy.Count == 0)
                {
                    outerColumn = subquery.GenerateOuterColumn(identifier);
                    _identifier.Add(outerColumn);
                }
            }

            var childIdentifiers = _childIdentifiers.ToList();
            _childIdentifiers.Clear();
            // TODO: See issue#15873
            foreach (var identifier in childIdentifiers)
            {
                if (projectionMap.TryGetValue(identifier, out var outerColumn))
                {
                    _childIdentifiers.Add(outerColumn);
                }
                else if (!IsDistinct
                    && GroupBy.Count == 0)
                {
                    outerColumn = subquery.GenerateOuterColumn(identifier);
                    _childIdentifiers.Add(outerColumn);
                }
            }

            var pendingCollections = _pendingCollections.ToList();
            _pendingCollections.Clear();
            _pendingCollections.AddRange(pendingCollections.Select(new SqlRemappingVisitor(projectionMap, subquery).Remap));

            _orderings.Clear();
            // Only lift order by to outer if subquery does not have distinct
            if (!subquery.IsDistinct)
            {
                foreach (var ordering in subquery._orderings)
                {
                    var orderingExpression = ordering.Expression;
                    if (!projectionMap.TryGetValue(orderingExpression, out var outerColumn))
                    {
                        outerColumn = subquery.GenerateOuterColumn(orderingExpression);
                    }

                    _orderings.Add(ordering.Update(outerColumn));
                }
            }

            if (subquery.Offset == null
                && subquery.Limit == null)
            {
                subquery.ClearOrdering();
            }

            Offset = null;
            Limit = null;
            IsDistinct = false;
            Predicate = null;
            Having = null;
            _tables.Clear();
            _tables.Add(subquery);
            _groupBy.Clear();

            return projectionMap;

            EntityProjectionExpression LiftEntityProjectionFromSubquery(EntityProjectionExpression entityProjection)
            {
                var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                {
                    var innerColumn = entityProjection.BindProperty(property);
                    var outerColumn = subquery.GenerateOuterColumn(innerColumn);
                    projectionMap[innerColumn] = outerColumn;
                    propertyExpressions[property] = outerColumn;
                }

                var newEntityProjection = new EntityProjectionExpression(entityProjection.EntityType, propertyExpressions);
                // Also lift nested entity projections
                foreach (var navigation in entityProjection.EntityType.GetTypesInHierarchy()
                    .SelectMany(Microsoft.EntityFrameworkCore.EntityTypeExtensions.GetDeclaredNavigations))
                {
                    var boundEntityShaperExpression = entityProjection.BindNavigation(navigation);
                    if (boundEntityShaperExpression != null)
                    {
                        var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                        var newInnerEntityProjection = LiftEntityProjectionFromSubquery(innerEntityProjection);
                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                        newEntityProjection.AddNavigationBinding(navigation, boundEntityShaperExpression);
                    }
                }

                return newEntityProjection;
            }
        }

        private enum JoinType
        {
            InnerJoin,
            LeftJoin,
            CrossJoin,
            CrossApply,
            OuterApply
        }

        private void AddJoin(
            JoinType joinType,
            SelectExpression innerSelectExpression,
            Type transparentIdentifierType,
            SqlExpression joinPredicate = null)
        {
            // Try to convert Apply to normal join
            if (joinType == JoinType.CrossApply
                || joinType == JoinType.OuterApply)
            {
                // Doing for limit only since limit + offset may need sum
                var limit = innerSelectExpression.Limit;
                innerSelectExpression.Limit = null;

                joinPredicate = TryExtractJoinKey(innerSelectExpression);
                if (joinPredicate != null)
                {
                    var containsOuterReference = new SelectExpressionCorrelationFindingExpressionVisitor(this)
                        .ContainsOuterReference(innerSelectExpression);
                    if (containsOuterReference)
                    {
                        innerSelectExpression.ApplyPredicate(joinPredicate);
                        innerSelectExpression.ApplyLimit(limit);
                    }
                    else
                    {
                        if (limit != null)
                        {
                            var partitions = new List<SqlExpression>();
                            GetPartitions(joinPredicate, partitions);
                            var orderings = innerSelectExpression.Orderings.Any()
                                ? innerSelectExpression.Orderings
                                : innerSelectExpression._identifier.Select(e => new OrderingExpression(e, true));
                            var rowNumberExpression = new RowNumberExpression(partitions, orderings.ToList(), limit.TypeMapping);
                            innerSelectExpression.ClearOrdering();

                            var projectionMappings = innerSelectExpression.PushdownIntoSubquery();
                            var subquery = (SelectExpression)innerSelectExpression.Tables[0];

                            joinPredicate = new SqlRemappingVisitor(
                                    projectionMappings, subquery)
                                .Remap(joinPredicate);

                            var outerColumn = subquery.GenerateOuterColumn(rowNumberExpression, "row");
                            var predicate = new SqlBinaryExpression(
                                ExpressionType.LessThanOrEqual, outerColumn, limit, typeof(bool), joinPredicate.TypeMapping);
                            innerSelectExpression.ApplyPredicate(predicate);
                        }

                        AddJoin(
                            joinType == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftJoin,
                            innerSelectExpression, transparentIdentifierType, joinPredicate);
                        return;
                    }
                }
                else
                {
                    innerSelectExpression.ApplyLimit(limit);
                }
            }

            // Verify what are the cases of pushdown for inner & outer both sides
            if (Limit != null
                || Offset != null
                || IsDistinct
                || GroupBy.Count > 0)
            {
                var sqlRemappingVisitor = new SqlRemappingVisitor(PushdownIntoSubquery(), (SelectExpression)Tables[0]);
                innerSelectExpression = sqlRemappingVisitor.Remap(innerSelectExpression);
                joinPredicate = sqlRemappingVisitor.Remap(joinPredicate);
            }

            if (innerSelectExpression.Orderings.Any()
                || innerSelectExpression.Limit != null
                || innerSelectExpression.Offset != null
                || innerSelectExpression.IsDistinct
                || innerSelectExpression.Predicate != null
                || innerSelectExpression.Tables.Count > 1
                || innerSelectExpression.GroupBy.Count > 0)
            {
                joinPredicate = new SqlRemappingVisitor(
                        innerSelectExpression.PushdownIntoSubquery(), (SelectExpression)innerSelectExpression.Tables[0])
                    .Remap(joinPredicate);
            }

            if (joinType != JoinType.LeftJoin)
            {
                _identifier.AddRange(innerSelectExpression._identifier);
            }

            var innerTable = innerSelectExpression.Tables.Single();
            var joinTable = joinType switch
            {
                JoinType.InnerJoin => new InnerJoinExpression(innerTable, joinPredicate),
                JoinType.LeftJoin => new LeftJoinExpression(innerTable, joinPredicate),
                JoinType.CrossJoin => new CrossJoinExpression(innerTable),
                JoinType.CrossApply => new CrossApplyExpression(innerTable),
                JoinType.OuterApply => (TableExpressionBase)new OuterApplyExpression(innerTable),
                _ => throw new InvalidOperationException($"Invalid {nameof(joinType)}: {joinType}")
            };

            _tables.Add(joinTable);

            if (transparentIdentifierType != null)
            {
                var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var projection in _projectionMapping)
                {
                    projectionMapping[projection.Key.Prepend(outerMemberInfo)] = projection.Value;
                }

                var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
                var innerNullable = joinType == JoinType.LeftJoin || joinType == JoinType.OuterApply;
                foreach (var projection in innerSelectExpression._projectionMapping)
                {
                    var projectionToAdd = projection.Value;
                    if (innerNullable)
                    {
                        if (projectionToAdd is EntityProjectionExpression entityProjection)
                        {
                            projectionToAdd = entityProjection.MakeNullable();
                        }
                        else if (projectionToAdd is ColumnExpression column)
                        {
                            projectionToAdd = column.MakeNullable();
                        }
                    }

                    projectionMapping[projection.Key.Prepend(innerMemberInfo)] = projectionToAdd;
                }

                _projectionMapping = projectionMapping;
            }
        }


        private SqlExpression TryExtractJoinKey(SelectExpression selectExpression)
        {
            if (selectExpression.Limit == null
                && selectExpression.Offset == null
                && selectExpression.Predicate != null)
            {
                var columnExpressions = new List<ColumnExpression>();
                var joinPredicate = TryExtractJoinKey(selectExpression, selectExpression.Predicate, columnExpressions, out var predicate);
                if (joinPredicate != null)
                {
                    joinPredicate = RemoveRedundantNullChecks(joinPredicate, columnExpressions);
                }

                selectExpression.Predicate = predicate;

                return joinPredicate;
            }

            return null;
        }

        private SqlExpression TryExtractJoinKey(
           SelectExpression selectExpression,
           SqlExpression predicate,
           List<ColumnExpression> columnExpressions,
           out SqlExpression updatedPredicate)
        {
            if (predicate is SqlBinaryExpression sqlBinaryExpression)
            {
                var joinPredicate = ValidateKeyComparison(selectExpression, sqlBinaryExpression, columnExpressions);
                if (joinPredicate != null)
                {
                    updatedPredicate = null;

                    return joinPredicate;
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    var leftJoinKey = TryExtractJoinKey(
                        selectExpression, sqlBinaryExpression.Left, columnExpressions, out var leftPredicate);
                    var rightJoinKey = TryExtractJoinKey(
                        selectExpression, sqlBinaryExpression.Right, columnExpressions, out var rightPredicate);

                    updatedPredicate = CombineNonNullExpressions(leftPredicate, rightPredicate);

                    return CombineNonNullExpressions(leftJoinKey, rightJoinKey);
                }
            }

            updatedPredicate = predicate;

            return null;
        }

        private static SqlExpression CombineNonNullExpressions(SqlExpression left, SqlExpression right)
           => left != null
               ? right != null
                   ? new SqlBinaryExpression(ExpressionType.AndAlso, left, right, left.Type, left.TypeMapping)
                   : left
               : right;

        private SqlBinaryExpression ValidateKeyComparison(
            SelectExpression inner, SqlBinaryExpression sqlBinaryExpression, List<ColumnExpression> columnExpressions)
        {
            if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
            {
                if (sqlBinaryExpression.Left is ColumnExpression leftColumn
                    && sqlBinaryExpression.Right is ColumnExpression rightColumn)
                {
                    if (ContainsTableReference(leftColumn.Table)
                        && inner.ContainsTableReference(rightColumn.Table))
                    {
                        columnExpressions.Add(leftColumn);

                        return sqlBinaryExpression;
                    }

                    if (ContainsTableReference(rightColumn.Table)
                        && inner.ContainsTableReference(leftColumn.Table))
                    {
                        columnExpressions.Add(rightColumn);

                        return sqlBinaryExpression.Update(
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left);
                    }
                }
            }

            // null checks are considered part of join key
            if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
            {
                if (sqlBinaryExpression.Left is ColumnExpression leftNullCheckColumn
                    && ContainsTableReference(leftNullCheckColumn.Table)
                    && sqlBinaryExpression.Right is SqlConstantExpression rightConstant
                    && rightConstant.Value == null)
                {
                    return sqlBinaryExpression;
                }

                if (sqlBinaryExpression.Right is ColumnExpression rightNullCheckColumn
                    && ContainsTableReference(rightNullCheckColumn.Table)
                    && sqlBinaryExpression.Left is SqlConstantExpression leftConstant
                    && leftConstant.Value == null)
                {
                    return sqlBinaryExpression.Update(
                        sqlBinaryExpression.Right,
                        sqlBinaryExpression.Left);
                }
            }

            return null;
        }

        private class SelectExpressionCorrelationFindingExpressionVisitor : ExpressionVisitor
        {
            private readonly SelectExpression _outerSelectExpression;
            private bool _containsOuterReference;

            private readonly bool _quirkMode19825;

            public SelectExpressionCorrelationFindingExpressionVisitor(SelectExpression outerSelectExpression)
            {
                _outerSelectExpression = outerSelectExpression;
                _quirkMode19825 = AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue19825", out var enabled) && enabled;
            }

            public bool ContainsOuterReference(SelectExpression selectExpression)
            {
                _containsOuterReference = false;

                Visit(selectExpression);

                return _containsOuterReference;
            }

            public override Expression Visit(Expression expression)
            {
                if (_containsOuterReference)
                {
                    return expression;
                }

                if (_quirkMode19825)
                {
                    if (expression is ColumnExpression columnExpression
                        && _outerSelectExpression.Tables.Contains(columnExpression.Table))
                    {
                        _containsOuterReference = true;

                        return expression;
                    }
                }
                else
                {
                    if (expression is ColumnExpression columnExpression
                        && _outerSelectExpression.ContainsTableReference(columnExpression.Table))
                    {
                        _containsOuterReference = true;

                        return expression;
                    }
                }

                return base.Visit(expression);
            }
        }

        private SqlExpression RemoveRedundantNullChecks(SqlExpression predicate, List<ColumnExpression> columnExpressions)
        {
            if (predicate is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                    && sqlBinaryExpression.Left is ColumnExpression leftColumn
                    && columnExpressions.Contains(leftColumn)
                    && sqlBinaryExpression.Right is SqlConstantExpression sqlConstantExpression
                    && sqlConstantExpression.Value == null)
                {
                    return null;
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    var leftPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Left, columnExpressions);
                    var rightPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Right, columnExpressions);

                    return CombineNonNullExpressions(leftPredicate, rightPredicate);
                }
            }

            return predicate;
        }

        private void GetPartitions(SqlExpression sqlExpression, List<SqlExpression> partitions)
        {
            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                {
                    partitions.Add(sqlBinaryExpression.Right);
                }
                else if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    GetPartitions(sqlBinaryExpression.Left, partitions);
                    GetPartitions(sqlBinaryExpression.Right, partitions);
                }
            }
        }

        private class SqlRemappingVisitor : ExpressionVisitor
        {
            private readonly SelectExpression _subquery;
            private readonly IDictionary<SqlExpression, ColumnExpression> _mappings;

            public SqlRemappingVisitor(IDictionary<SqlExpression, ColumnExpression> mappings, SelectExpression subquery)
            {
                _subquery = subquery;
                _mappings = mappings;
            }

            public SqlExpression Remap(SqlExpression sqlExpression) => (SqlExpression)Visit(sqlExpression);
            public SelectExpression Remap(SelectExpression sqlExpression) => (SelectExpression)Visit(sqlExpression);

            public override Expression Visit(Expression expression)
            {
                switch (expression)
                {
                    case SqlExpression sqlExpression
                        when _mappings.TryGetValue(sqlExpression, out var outer):
                        return outer;

                    case ColumnExpression columnExpression
                        when _subquery.ContainsTableReference(columnExpression.Table):
                        var index = _subquery.AddToProjection(columnExpression);
                        var projectionExpression = _subquery._projection[index];
                        return new ColumnExpression(projectionExpression, _subquery);

                    default:
                        return base.Visit(expression);
                }
            }
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("Projection Mapping:");
            using (expressionPrinter.Indent())
            {
                foreach (var projectionMappingEntry in _projectionMapping)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(projectionMappingEntry.Key + " -> ");
                    expressionPrinter.Visit(projectionMappingEntry.Value);
                }
            }

            expressionPrinter.AppendLine();

            foreach (var tag in Tags)
            {
                expressionPrinter.Append($"-- {tag}");
            }

            IDisposable indent = null;

            if (Alias != null)
            {
                expressionPrinter.AppendLine("(");
                indent = expressionPrinter.Indent();
            }

            expressionPrinter.Append("SELECT ");

            if (IsDistinct)
            {
                expressionPrinter.Append("DISTINCT ");
            }

            if (Limit != null
                && Offset == null)
            {
                expressionPrinter.Append("TOP(");
                expressionPrinter.Visit(Limit);
                expressionPrinter.Append(") ");
            }

            if (Projection.Any())
            {
                expressionPrinter.VisitList(Projection);
            }
            else
            {
                expressionPrinter.Append("1");
            }

            if (Tables.Any())
            {
                expressionPrinter.AppendLine().Append("FROM ");

                expressionPrinter.VisitList(Tables, p => p.AppendLine());
            }

            if (Predicate != null)
            {
                expressionPrinter.AppendLine().Append("WHERE ");
                expressionPrinter.Visit(Predicate);
            }

            if (GroupBy.Any())
            {
                expressionPrinter.AppendLine().Append("GROUP BY ");
                expressionPrinter.VisitList(GroupBy);
            }

            if (Having != null)
            {
                expressionPrinter.AppendLine().Append("HAVING ");
                expressionPrinter.Visit(Having);
            }

            if (Orderings.Any())
            {
                expressionPrinter.AppendLine().Append("ORDER BY ");
                expressionPrinter.VisitList(Orderings);
            }
            else if (Offset != null)
            {
                expressionPrinter.AppendLine().Append("ORDER BY (SELECT 1)");
            }

            if (Offset != null)
            {
                expressionPrinter.AppendLine().Append("OFFSET ");
                expressionPrinter.Visit(Offset);
                expressionPrinter.Append(" ROWS");

                if (Limit != null)
                {
                    expressionPrinter.Append(" FETCH NEXT ");
                    expressionPrinter.Visit(Limit);
                    expressionPrinter.Append(" ROWS ONLY");
                }
            }

            if (Alias != null)
            {
                indent?.Dispose();
                expressionPrinter.AppendLine().Append(") AS " + Alias);
            }
        }
    }
}
