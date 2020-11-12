using AutoPocoIO.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal partial class SqlExpressionFactory : Microsoft.EntityFrameworkCore.Query.SqlExpressionFactory, ISqlExpressionFactoryWithCrossDb
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _boolTypeMapping;

        public SqlExpressionFactory(SqlExpressionFactoryDependencies dependencies) : base(dependencies)
        {
            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
        }

        public SelectExpression SelectWithCrossDb(IEntityType entityType)
        {
            var selectExpression = new SelectExpression(entityType);
            AddConditions(selectExpression, entityType);

            return selectExpression;
        }

        private void AddInnerJoin(
            SelectExpression selectExpression, IForeignKey foreignKey, ICollection<IEntityType> sharingTypes, bool skipInnerJoins)
        {
            var joinPredicate = GenerateJoinPredicate(selectExpression, foreignKey, sharingTypes, skipInnerJoins, out var innerSelect);

            selectExpression.AddInnerJoin(innerSelect, joinPredicate, null);
        }

        private SqlExpression GenerateJoinPredicate(
          SelectExpression selectExpression,
          IForeignKey foreignKey,
          ICollection<IEntityType> sharingTypes,
          bool skipInnerJoins,
          out SelectExpression innerSelect)
        {
            var outerEntityProjection = GetMappedEntityProjectionExpression(selectExpression);
            var outerIsPrincipal = foreignKey.PrincipalEntityType.IsAssignableFrom(outerEntityProjection.EntityType);

            innerSelect = outerIsPrincipal
                ? new SelectExpression(foreignKey.DeclaringEntityType)
                : new SelectExpression(foreignKey.PrincipalEntityType);
            AddConditions(
                innerSelect,
                outerIsPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType,
                sharingTypes,
                skipInnerJoins);

            var innerEntityProjection = GetMappedEntityProjectionExpression(innerSelect);

            var outerKey = (outerIsPrincipal ? foreignKey.PrincipalKey.Properties : foreignKey.Properties)
                .Select(p => outerEntityProjection.BindProperty(p));
            var innerKey = (outerIsPrincipal ? foreignKey.Properties : foreignKey.PrincipalKey.Properties)
                .Select(p => innerEntityProjection.BindProperty(p));

            return outerKey.Zip<SqlExpression, SqlExpression, SqlExpression>(innerKey, Equal)
                .Aggregate(AndAlso);
        }

        private void AddConditions(
          SelectExpression selectExpression,
          IEntityType entityType,
          ICollection<IEntityType> sharingTypes = null,
          bool skipJoins = false)
        {
            if (entityType.FindPrimaryKey() == null)
            {
                AddDiscriminatorCondition(selectExpression, entityType);
            }
            else
            {
                sharingTypes ??= new HashSet<IEntityType>(
                    entityType.Model.GetEntityTypes()
                        .Where(
                            et => et.FindPrimaryKey() != null
                                && et.GetTableName() == entityType.GetTableName()
                                && et.GetSchema() == entityType.GetSchema()));

                if (sharingTypes.Count > 0)
                {
                    var discriminatorAdded = AddDiscriminatorCondition(selectExpression, entityType);

                    var linkingFks = entityType.GetRootType().FindForeignKeys(entityType.FindPrimaryKey().Properties)
                        .Where(
                            fk => fk.PrincipalKey.IsPrimaryKey()
                                && fk.PrincipalEntityType != entityType
                                && sharingTypes.Contains(fk.PrincipalEntityType))
                        .ToList();

                    if (linkingFks.Count > 0)
                    {
                        if (!discriminatorAdded)
                        {
                            AddOptionalDependentConditions(selectExpression, entityType, sharingTypes);
                        }

                        if (!skipJoins)
                        {
                            if (AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue18299", out var isEnabled) && isEnabled)
                            {
                                AddInnerJoin(selectExpression, linkingFks[0], sharingTypes, skipInnerJoins: false);

                                foreach (var otherFk in linkingFks.Skip(1))
                                {
                                    var otherSelectExpression = new SelectExpression(entityType);

                                    AddInnerJoin(otherSelectExpression, otherFk, sharingTypes, skipInnerJoins: false);
                                    selectExpression.ApplyUnion(otherSelectExpression, distinct: true);
                                }
                            }
                            else
                            {
                                var first = true;

                                foreach (var foreignKey in linkingFks)
                                {
                                    if (!(entityType.FindOwnership() == foreignKey
                                        && foreignKey.PrincipalEntityType.BaseType == null))
                                    {
                                        var otherSelectExpression = first
                                            ? selectExpression
                                            : new SelectExpression(entityType);

                                        AddInnerJoin(otherSelectExpression, foreignKey, sharingTypes, skipInnerJoins: false);

                                        if (first)
                                        {
                                            first = false;
                                        }
                                        else
                                        {
                                            selectExpression.ApplyUnion(otherSelectExpression, distinct: true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool AddDiscriminatorCondition(SelectExpression selectExpression, IEntityType entityType)
        {
            SqlExpression predicate;
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
            if (concreteEntityTypes.Count == 1)
            {
                var concreteEntityType = concreteEntityTypes[0];
                if (concreteEntityType.BaseType == null)
                {
                    return false;
                }

                var discriminatorColumn = GetMappedEntityProjectionExpression(selectExpression)
                    .BindProperty(concreteEntityType.GetDiscriminatorProperty());

                predicate = Equal(discriminatorColumn, Constant(concreteEntityType.GetDiscriminatorValue()));
            }
            else
            {
                var discriminatorColumn = GetMappedEntityProjectionExpression(selectExpression)
                    .BindProperty(concreteEntityTypes[0].GetDiscriminatorProperty());

                predicate = In(
                    discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()), negated: false);
            }

            selectExpression.ApplyPredicate(predicate);

            return true;
        }

        private void AddOptionalDependentConditions(
           SelectExpression selectExpression, IEntityType entityType, ICollection<IEntityType> sharingTypes)
        {
            SqlExpression predicate = null;
            var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
            if (requiredNonPkProperties.Count > 0)
            {
                var entityProjectionExpression = GetMappedEntityProjectionExpression(selectExpression);
                predicate = IsNotNull(requiredNonPkProperties[0], entityProjectionExpression);

                if (requiredNonPkProperties.Count > 1)
                {
                    predicate
                        = requiredNonPkProperties
                            .Skip(1)
                            .Aggregate(
                                predicate, (current, property) =>
                                    AndAlso(
                                        IsNotNull(property, entityProjectionExpression),
                                        current));
                }

                selectExpression.ApplyPredicate(predicate);
            }
            else
            {
                var allNonPkProperties = entityType.GetProperties().Where(p => !p.IsPrimaryKey()).ToList();
                if (allNonPkProperties.Count > 0)
                {
                    var entityProjectionExpression = GetMappedEntityProjectionExpression(selectExpression);
                    predicate = IsNotNull(allNonPkProperties[0], entityProjectionExpression);

                    if (allNonPkProperties.Count > 1)
                    {
                        predicate
                            = allNonPkProperties
                                .Skip(1)
                                .Aggregate(
                                    predicate, (current, property) =>
                                        OrElse(
                                            IsNotNull(property, entityProjectionExpression),
                                            current));
                    }

                    selectExpression.ApplyPredicate(predicate);

                    foreach (var referencingFk in entityType.GetReferencingForeignKeys())
                    {
                        var otherSelectExpression = new SelectExpression(entityType);

                        var sameTable = sharingTypes.Contains(referencingFk.DeclaringEntityType);
                        AddInnerJoin(
                            otherSelectExpression, referencingFk,
                            sameTable ? sharingTypes : null,
                            skipInnerJoins: sameTable);
                        selectExpression.ApplyUnion(otherSelectExpression, distinct: true);
                    }
                }
            }
        }

        private static EntityProjectionExpression GetMappedEntityProjectionExpression(SelectExpression selectExpression)
           => (EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember());

        private SqlExpression IsNotNull(IProperty property, EntityProjectionExpression entityProjection)
            => IsNotNull(entityProjection.BindProperty(property));

        public override SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            return base.ApplyDefaultTypeMapping(sqlExpression);
        }

        public override SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
        {
            return base.ApplyTypeMapping(sqlExpression, typeMapping);
        }

        public override RelationalTypeMapping GetTypeMappingForValue(object value)
        {
            return base.GetTypeMappingForValue(value);
        }

        public override RelationalTypeMapping FindMapping(Type type)
        {
            return base.FindMapping(type);
        }

        public override SqlBinaryExpression MakeBinary(ExpressionType operatorType, SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping)
        {
            return base.MakeBinary(operatorType, left, right, typeMapping);
        }

        public override SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        {
            return base.Equal(left, right);
        }

        public override SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        {
            return base.NotEqual(left, right);
        }

        public override SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        {
            return base.GreaterThan(left, right);
        }

        public override SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return base.GreaterThanOrEqual(left, right);
        }

        public override SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        {
            return base.LessThan(left, right);
        }

        public override SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return base.LessThanOrEqual(left, right);
        }

        public override SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        {
            return base.AndAlso(left, right);
        }

        public override SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        {
            return base.OrElse(left, right);
        }

        public override SqlBinaryExpression Add(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.Add(left, right, typeMapping);
        }

        public override SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.Subtract(left, right, typeMapping);
        }

        public override SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.Multiply(left, right, typeMapping);
        }

        public override SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.Divide(left, right, typeMapping);
        }

        public override SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.Modulo(left, right, typeMapping);
        }

        public override SqlBinaryExpression And(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.And(left, right, typeMapping);
        }

        public override SqlBinaryExpression Or(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.Or(left, right, typeMapping);
        }

        public override SqlBinaryExpression Coalesce(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return base.Coalesce(left, right, typeMapping);
        }

        public override SqlUnaryExpression MakeUnary(ExpressionType operatorType, SqlExpression operand, Type type, RelationalTypeMapping typeMapping = null)
        {
            return base.MakeUnary(operatorType, operand, type, typeMapping);
        }

        public override SqlUnaryExpression IsNull(SqlExpression operand)
        {
            return base.IsNull(operand);
        }

        public override SqlUnaryExpression IsNotNull(SqlExpression operand)
        {
            return base.IsNotNull(operand);
        }

        public override SqlUnaryExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping typeMapping = null)
        {
            return base.Convert(operand, type, typeMapping);
        }

        public override SqlUnaryExpression Not(SqlExpression operand)
        {
            return base.Not(operand);
        }

        public override SqlUnaryExpression Negate(SqlExpression operand)
        {
            return base.Negate(operand);
        }

        public override CaseExpression Case(SqlExpression operand, SqlExpression elseResult, params CaseWhenClause[] whenClauses)
        {
            return base.Case(operand, elseResult, whenClauses);
        }

        public override CaseExpression Case(SqlExpression operand, params CaseWhenClause[] whenClauses)
        {
            return base.Case(operand, whenClauses);
        }

        public override CaseExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression elseResult)
        {
            return base.Case(whenClauses, elseResult);
        }

        public override SqlFunctionExpression Function(string name, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return base.Function(name, arguments, returnType, typeMapping);
        }

        public override SqlFunctionExpression Function(string schema, string name, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return base.Function(schema, name, arguments, returnType, typeMapping);
        }

        public override SqlFunctionExpression Function(SqlExpression instance, string name, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return base.Function(instance, name, arguments, returnType, typeMapping);
        }

        public override SqlFunctionExpression Function(string name, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return base.Function(name, returnType, typeMapping);
        }

        public override SqlFunctionExpression Function(string schema, string name, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return base.Function(schema, name, returnType, typeMapping);
        }

        public override SqlFunctionExpression Function(SqlExpression instance, string name, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return base.Function(instance, name, returnType, typeMapping);
        }

        public ExistsExpression Exists(SelectExpression subquery, bool negated)
         => new ExistsExpression(subquery, negated, _boolTypeMapping);

        public new InExpression In(SqlExpression item, SqlExpression values, bool negated)
        {
            var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type);

            item = ApplyTypeMapping(item, typeMapping);
            values = ApplyTypeMapping(values, typeMapping);
            return new InExpression(item, negated, values, _boolTypeMapping);

        }

        public InExpression In(SqlExpression item, SelectExpression subquery, bool negated)
        {
            var sqlExpression = subquery.Projection.Single().Expression;
            var typeMapping = sqlExpression.TypeMapping;

            if (typeMapping == null)
            {
                throw new InvalidOperationException(
                    $"The subquery '{subquery.Print()}' references type '{sqlExpression.Type}' for which no type mapping could be found.");
            }

            item = ApplyTypeMapping(item, typeMapping);
            return new InExpression(item, negated, subquery, _boolTypeMapping);
        }

        public override LikeExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar = null)
        {
            return base.Like(match, pattern, escapeChar);
        }

        public override SqlFragmentExpression Fragment(string sql)
        {
            return base.Fragment(sql);
        }

        public override SqlConstantExpression Constant(object value, RelationalTypeMapping typeMapping = null)
        {
            return base.Constant(value, typeMapping);
        }

        public override Microsoft.EntityFrameworkCore.Query.SqlExpressions.SelectExpression Select(SqlExpression projection)
        {
            return base.Select(projection);
        }

        public override Microsoft.EntityFrameworkCore.Query.SqlExpressions.SelectExpression Select(IEntityType entityType)
        {
            return base.Select(entityType);
        }

        public override Microsoft.EntityFrameworkCore.Query.SqlExpressions.SelectExpression Select(IEntityType entityType, string sql, Expression sqlArguments)
        {
            return base.Select(entityType, sql, sqlArguments);
        }
    }
}
