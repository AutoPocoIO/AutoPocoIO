using AutoPocoIO.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal partial class SqlExpressionFactory : Microsoft.EntityFrameworkCore.Query.SqlExpressionFactory
    {
        public SqlExpressionFactory(SqlExpressionFactoryDependencies dependencies) : base(dependencies)
        {
        }

        public new SelectExpression Select(IEntityType entityType)
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

        private EntityProjectionExpression GetMappedEntityProjectionExpression(SelectExpression selectExpression)
           => (EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember());

        private SqlExpression IsNotNull(IProperty property, EntityProjectionExpression entityProjection)
            => IsNotNull(entityProjection.BindProperty(property));
    }
}
