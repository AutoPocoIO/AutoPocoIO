using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    // Copyright (c) .NET Foundation. All rights reserved.
    // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.



    /// <summary>
    ///     The default relational <see cref="QueryModel" /> visitor.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class RelationalQueryModelVisitor : Microsoft.EntityFrameworkCore.Query.RelationalQueryModelVisitor
    {
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly EntityQueryModelVisitorDependencies _dependencies;
        private readonly RelationalQueryCompilationContext _queryCompilationContext;
        private delegate void DelVisitQueryModel(QueryModel queryModel);

        public RelationalQueryModelVisitor(EntityQueryModelVisitorDependencies dependencies,
            RelationalQueryModelVisitorDependencies relationalDependencies,
            RelationalQueryCompilationContext queryCompilationContext,
            Microsoft.EntityFrameworkCore.Query.RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(dependencies, relationalDependencies, queryCompilationContext, parentQueryModelVisitor)
        {
            _compositePredicateExpressionVisitorFactory = relationalDependencies.CompositePredicateExpressionVisitorFactory;
            _dependencies = dependencies;
            _queryCompilationContext = queryCompilationContext;
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            //Skip base and call base.baseVisitQueryModel(queryModel)
            MethodInfo method = typeof(EntityQueryModelVisitor).GetMethod("VisitQueryModel");

            EntityQueryModelVisitorHolder basebaseClass = new EntityQueryModelVisitorHolder(_dependencies, _queryCompilationContext);
            DelVisitQueryModel delVisitQueryModel = (DelVisitQueryModel)Delegate.CreateDelegate(typeof(DelVisitQueryModel), basebaseClass, "VisitQueryModel");
            delVisitQueryModel.GetType().BaseType.BaseType.GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(delVisitQueryModel, this);
            delVisitQueryModel(queryModel);

            var joinEliminator = new JoinEliminator();
            var compositePredicateVisitor = _compositePredicateExpressionVisitorFactory.Create();
            _ = RelationalOptionsExtension.Extract(ContextOptions).UseRelationalNulls;
            foreach (var selectExpression in QueriesBySource.Values)
            {
                joinEliminator.EliminateJoins(selectExpression as SelectExpression);
                compositePredicateVisitor.Visit(selectExpression);
            }
        }



        private class EntityQueryModelVisitorHolder : EntityQueryModelVisitor
        {
            public EntityQueryModelVisitorHolder(EntityQueryModelVisitorDependencies dependencies, QueryCompilationContext queryCompilationContext)
                : base(dependencies, queryCompilationContext)
            {
            }

        }


        private class RelationalNullsMarkingExpressionVisitor : RelinqExpressionVisitor
        {
            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Equal
                    || binaryExpression.NodeType == ExpressionType.NotEqual)
                {
                    return new NullCompensatedExpression(binaryExpression);
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is NullCompensatedExpression)
                {
                    return extensionExpression;
                }

                if (extensionExpression is SelectExpression selectExpression)
                {
                    if (selectExpression.Projection.Any())
                    {
                        var projectionsChanged = default(bool);
                        var newProjections = new List<Expression>();
                        foreach (var projection in selectExpression.Projection)
                        {
                            var newProjection = Visit(projection);
                            if (newProjection != projection)
                            {
                                projectionsChanged = true;
                            }

                            newProjections.Add(newProjection);
                        }

                        if (projectionsChanged)
                        {
                            selectExpression.ClearProjection();
                            foreach (var newProjection in newProjections)
                            {
                                selectExpression.AddToProjection(newProjection);
                            }
                        }
                    }

                    if (selectExpression.Predicate != null)
                    {
                        selectExpression.Predicate = Visit(selectExpression.Predicate);
                    }

                    if (selectExpression.GroupBy.Any())
                    {
                        var groupByChanged = default(bool);
                        var newGroupBys = new List<Expression>();
                        foreach (var groupBy in selectExpression.GroupBy)
                        {
                            var newGroupBy = Visit(groupBy);
                            if (newGroupBy != groupBy)
                            {
                                groupByChanged = true;
                            }

                            newGroupBys.Add(newGroupBy);
                        }

                        if (groupByChanged)
                        {
                            //  selectExpression.ClearGroupBy();
                            selectExpression.AddToGroupBy(newGroupBys.ToArray());
                        }
                    }

                    if (selectExpression.Having != null)
                    {
                        selectExpression.Having = Visit(selectExpression.Having);
                    }

                    if (selectExpression.OrderBy.Any())
                    {
                        var orderByChanged = default(bool);
                        var newOrderings = new List<Ordering>();
                        foreach (var ordering in selectExpression.OrderBy)
                        {
                            var newOrdering = ordering;
                            var newOrderBy = Visit(ordering.Expression);
                            if (newOrderBy != ordering.Expression)
                            {
                                orderByChanged = true;
                                newOrdering = new Ordering(newOrderBy, ordering.OrderingDirection);
                            }

                            newOrderings.Add(newOrdering);
                        }

                        if (orderByChanged)
                        {
                            selectExpression.ClearOrderBy();
                            foreach (var newOrdering in newOrderings)
                            {
                                selectExpression.AddToOrderBy(newOrdering);
                            }
                        }
                    }
                }

                return base.VisitExtension(extensionExpression);
            }
        }

        private class JoinEliminator : ExpressionVisitor
        {
            private readonly List<TableExpression> _tables = new List<TableExpression>();
            private readonly List<ColumnExpression> _columns = new List<ColumnExpression>();

            private bool _canEliminate;

            public void EliminateJoins(SelectExpression selectExpression)
            {
                if (selectExpression == null)
                    return;
                for (var i = selectExpression.Tables.Count - 1; i >= 0; i--)
                {
                    var tableExpression = selectExpression.Tables[i];

                    if (tableExpression is LeftOuterJoinExpression joinExpressionBase)
                    {
                        _tables.Clear();
                        _columns.Clear();
                        _canEliminate = true;

                        Visit(joinExpressionBase.Predicate);

                        if (_canEliminate
                            && _columns.Count > 0
                            && _columns.Count % 2 == 0
                            && _tables.Count == 2
                            && _tables[0].DatabaseName == _tables[1].DatabaseName
                            && _tables[0].Table == _tables[1].Table
                            && _tables[0].Schema == _tables[1].Schema)
                        {
                            for (var j = 0; j < _columns.Count - 1; j += 2)
                            {
                                if (_columns[j].Name != _columns[j + 1].Name)
                                {
                                    _canEliminate = false;

                                    break;
                                }
                            }

                            if (_canEliminate)
                            {
                                var newTableExpression
                                    = _tables.Single(t => !ReferenceEquals(t, joinExpressionBase.TableExpression));

                                selectExpression.RemoveTable(joinExpressionBase);
                                if (ReferenceEquals(selectExpression.ProjectStarTable, joinExpressionBase))
                                {
                                    selectExpression.ProjectStarTable
                                        = selectExpression.GetTableForQuerySource(newTableExpression.QuerySource);
                                }

                                //  selectExpression.UpdateTableReference(joinExpressionBase.TableExpression, newTableExpression);
                            }
                        }
                    }
                }
            }
        }
    }


}
