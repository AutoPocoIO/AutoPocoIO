﻿using AutoPocoIO.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal class QuerySqlGeneratorWithCrossDb : Microsoft.EntityFrameworkCore.Query.QuerySqlGenerator
    {
        private static readonly Regex _composableSql
      = new Regex(@"^\s*?SELECT\b", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(value: 1000.0));

        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private IRelationalCommandBuilder _relationalCommandBuilder;

        public QuerySqlGeneratorWithCrossDb(QuerySqlGeneratorDependencies dependencies) : base(dependencies)
        {
            _relationalCommandBuilderFactory = dependencies.RelationalCommandBuilderFactory;
            _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case ColumnExpression columnExpression:
                    return VisitColumn(columnExpression);
                case ExceptExpression exceptExpression:
                    return VisitExcept(exceptExpression);
                case ExistsExpression existsExpression:
                    return VisitExists(existsExpression);
                case InExpression inExpression:
                    return VisitIn(inExpression);
                case IntersectExpression intersectExpression:
                    return VisitIntersect(intersectExpression);
                case SelectExpression selectExpression:
                    return VisitSelect(selectExpression);
                case ScalarSubqueryExpression subSelectExpression:
                    return VisitSubSelect(subSelectExpression);
                case TableExpression tableExpression:
                    return VisitTable(tableExpression);
                case UnionExpression unionExpression:
                    return VisitUnion(unionExpression);
            }
            return base.VisitExtension(extensionExpression);
        }

        protected override IRelationalCommandBuilder Sql => _relationalCommandBuilder;

        public virtual IRelationalCommand GetCommand(SelectExpression selectExpression)
        {
            _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            GenerateTagsHeaderComment(selectExpression);

            if (selectExpression.IsNonComposedFromSql())
            {
                GenerateFromSql((FromSqlExpression)selectExpression.Tables[0]);
            }
            else
            {
                VisitSelect(selectExpression);
            }

            return _relationalCommandBuilder.Build();
        }

        protected virtual void GenerateTagsHeaderComment(SelectExpression selectExpression)
        {
            if (selectExpression.Tags.Count > 0)
            {
                foreach (var tag in selectExpression.Tags)
                {
                    _relationalCommandBuilder
                        .AppendLines(_sqlGenerationHelper.GenerateComment(tag))
                        .AppendLine();
                }
            }
        }

        private void GenerateFromSql(FromSqlExpression fromSqlExpression)
        {
            var sql = fromSqlExpression.Sql;
            string[] substitutions = null;

            switch (fromSqlExpression.Arguments)
            {
                case ConstantExpression constantExpression
                    when constantExpression.Value is CompositeRelationalParameter compositeRelationalParameter:
                    {
                        var subParameters = compositeRelationalParameter.RelationalParameters;
                        substitutions = new string[subParameters.Count];
                        for (var i = 0; i < subParameters.Count; i++)
                        {
                            substitutions[i] = _sqlGenerationHelper.GenerateParameterNamePlaceholder(subParameters[i].InvariantName);
                        }

                        _relationalCommandBuilder.AddParameter(compositeRelationalParameter);

                        break;
                    }

                case ConstantExpression constantExpression
                    when constantExpression.Value is object[] constantValues:
                    {
                        substitutions = new string[constantValues.Length];
                        for (var i = 0; i < constantValues.Length; i++)
                        {
                            var value = constantValues[i];
                            if (value is RawRelationalParameter rawRelationalParameter)
                            {
                                substitutions[i] = _sqlGenerationHelper.GenerateParameterNamePlaceholder(rawRelationalParameter.InvariantName);
                                _relationalCommandBuilder.AddParameter(rawRelationalParameter);
                            }
                            else if (value is SqlConstantExpression sqlConstantExpression)
                            {
                                substitutions[i] = sqlConstantExpression.TypeMapping.GenerateSqlLiteral(sqlConstantExpression.Value);
                            }
                        }

                        break;
                    }
            }

            if (substitutions != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                // InvariantCulture not needed since substitutions are all strings
                sql = string.Format(sql, substitutions);
            }

            _relationalCommandBuilder.AppendLines(sql);
        }

        protected Expression VisitSelect(SelectExpression selectExpression)
        {
            if (IsNonComposedSetOperation(selectExpression))
            {
                // Naked set operation
                GenerateSetOperation((SetOperationBase)selectExpression.Tables[0]);

                return selectExpression;
            }

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                _relationalCommandBuilder.AppendLine("(");
                subQueryIndent = _relationalCommandBuilder.Indent();
            }

            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _relationalCommandBuilder.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            if (selectExpression.Projection.Any())
            {
                GenerateList(selectExpression.Projection, e => Visit(e));
            }
            else
            {
                _relationalCommandBuilder.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _relationalCommandBuilder.AppendLine().Append("FROM ");

                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
            }

            if (selectExpression.Predicate != null)
            {
                _relationalCommandBuilder.AppendLine().Append("WHERE ");

                Visit(selectExpression.Predicate);
            }

            if (selectExpression.GroupBy.Count > 0)
            {
                _relationalCommandBuilder.AppendLine().Append("GROUP BY ");

                GenerateList(selectExpression.GroupBy, e => Visit(e));
            }

            if (selectExpression.Having != null)
            {
                _relationalCommandBuilder.AppendLine().Append("HAVING ");

                Visit(selectExpression.Having);
            }

            GenerateOrderings(selectExpression);
            GenerateLimitOffset(selectExpression);

            if (selectExpression.Alias != null)
            {
                subQueryIndent.Dispose();

                _relationalCommandBuilder.AppendLine()
                    .Append(")" + AliasSeparator + _sqlGenerationHelper.DelimitIdentifier(selectExpression.Alias));
            }

            return selectExpression;
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Visit(projectionExpression.Expression);

            if (!string.Equals(string.Empty, projectionExpression.Alias)
                && !(projectionExpression.Expression is ColumnExpression column
                    && string.Equals(column.Name, projectionExpression.Alias)))
            {
                _relationalCommandBuilder.Append(AliasSeparator + _sqlGenerationHelper.DelimitIdentifier(projectionExpression.Alias));
            }

            return projectionExpression;
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.IsBuiltIn)
            {
                if (sqlFunctionExpression.Instance != null)
                {
                    Visit(sqlFunctionExpression.Instance);
                    _relationalCommandBuilder.Append(".");
                }

                _relationalCommandBuilder.Append(sqlFunctionExpression.Name);
            }
            else
            {
                if (!string.IsNullOrEmpty(sqlFunctionExpression.Schema))
                {
                    _relationalCommandBuilder
                        .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Schema))
                        .Append(".");
                }

                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Name));
            }

            if (!sqlFunctionExpression.IsNiladic)
            {
                _relationalCommandBuilder.Append("(");
                GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }

            return sqlFunctionExpression;
        }

        protected Expression VisitColumn(ColumnExpression columnExpression)
        {
            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Table.Alias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        protected virtual Expression VisitTable(TableExpression tableExpression)
        {
            if (!string.IsNullOrEmpty(tableExpression.DatabaseName))
                _relationalCommandBuilder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableExpression.DatabaseName) + ".");


            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema))
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            var parameterNameInCommand = _sqlGenerationHelper.GenerateParameterName(sqlParameterExpression.Name);

            if (_relationalCommandBuilder.Parameters
                .All(p => p.InvariantName != sqlParameterExpression.Name))
            {
                _relationalCommandBuilder.AddParameter(
                    sqlParameterExpression.Name,
                    parameterNameInCommand,
                    sqlParameterExpression.TypeMapping,
                    sqlParameterExpression.Type.IsNullableType());
            }

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.GenerateParameterNamePlaceholder(sqlParameterExpression.Name));

            return sqlParameterExpression;
        }

        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            _relationalCommandBuilder.AppendLine("(");

            if (!_composableSql.IsMatch(fromSqlExpression.Sql))
            {
                throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
            }

            using (_relationalCommandBuilder.Indent())
            {
                GenerateFromSql(fromSqlExpression);
            }

            _relationalCommandBuilder.Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            if (sqlBinaryExpression.OperatorType == ExpressionType.Coalesce)
            {
                _relationalCommandBuilder.Append("COALESCE(");
                Visit(sqlBinaryExpression.Left);
                _relationalCommandBuilder.Append(", ");
                Visit(sqlBinaryExpression.Right);
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                var requiresBrackets = RequiresBrackets(sqlBinaryExpression.Left);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlBinaryExpression.Left);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }

                _relationalCommandBuilder.Append(GenerateOperator(sqlBinaryExpression));

                requiresBrackets = RequiresBrackets(sqlBinaryExpression.Right);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlBinaryExpression.Right);

                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }
            }

            return sqlBinaryExpression;
        }

        private bool RequiresBrackets(SqlExpression expression)
        {
            return expression is SqlBinaryExpression sqlBinary
                && sqlBinary.OperatorType != ExpressionType.Coalesce
                || expression is LikeExpression;
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            if (orderingExpression.Expression is SqlConstantExpression
                || orderingExpression.Expression is SqlParameterExpression)
            {
                _relationalCommandBuilder.Append("(SELECT 1)");
            }
            else
            {
                Visit(orderingExpression.Expression);
            }

            if (!orderingExpression.IsAscending)
            {
                _relationalCommandBuilder.Append(" DESC");
            }

            return orderingExpression;
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            Visit(likeExpression.Match);
            _relationalCommandBuilder.Append(" LIKE ");
            Visit(likeExpression.Pattern);

            if (likeExpression.EscapeChar != null)
            {
                _relationalCommandBuilder.Append(" ESCAPE ");
                Visit(likeExpression.EscapeChar);
            }

            return likeExpression;
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            _relationalCommandBuilder.Append("CASE");

            if (caseExpression.Operand != null)
            {
                _relationalCommandBuilder.Append(" ");
                Visit(caseExpression.Operand);
            }

            using (_relationalCommandBuilder.Indent())
            {
                foreach (var whenClause in caseExpression.WhenClauses)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("WHEN ");
                    Visit(whenClause.Test);
                    _relationalCommandBuilder.Append(" THEN ");
                    Visit(whenClause.Result);
                }

                if (caseExpression.ElseResult != null)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("ELSE ");
                    Visit(caseExpression.ElseResult);
                }
            }

            _relationalCommandBuilder
                .AppendLine()
                .Append("END");

            return caseExpression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Convert:
                    {
                        _relationalCommandBuilder.Append("CAST(");
                        var requiresBrackets = RequiresBrackets(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append("(");
                        }

                        Visit(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append(")");
                        }

                        _relationalCommandBuilder.Append(" AS ");
                        _relationalCommandBuilder.Append(sqlUnaryExpression.TypeMapping.StoreType);
                        _relationalCommandBuilder.Append(")");
                        break;
                    }

                case ExpressionType.Not:
                    {
                        _relationalCommandBuilder.Append("NOT (");
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(")");
                        break;
                    }

                case ExpressionType.Equal:
                    {
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(" IS NULL");
                        break;
                    }

                case ExpressionType.NotEqual:
                    {
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(" IS NOT NULL");
                        break;
                    }

                case ExpressionType.Negate:
                    {
                        _relationalCommandBuilder.Append("-");
                        Visit(sqlUnaryExpression.Operand);
                        break;
                    }
            }

            return sqlUnaryExpression;
        }

        protected Expression VisitExists(ExistsExpression existsExpression)
        {
            if (existsExpression.IsNegated)
            {
                _relationalCommandBuilder.Append("NOT ");
            }

            _relationalCommandBuilder.AppendLine("EXISTS (");

            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return existsExpression;
        }

        protected Expression VisitIn(InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.Append("(");
                var valuesConstant = (SqlConstantExpression)inExpression.Values;
                var valuesList = ((IEnumerable<object>)valuesConstant.Value)
                    .Select(v => new SqlConstantExpression(Expression.Constant(v), valuesConstant.TypeMapping)).ToList();
                GenerateList(valuesList, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.AppendLine("(");

                using (_relationalCommandBuilder.Indent())
                {
                    Visit(inExpression.Subquery);
                }

                _relationalCommandBuilder.AppendLine().AppendLine(")");
            }

            return inExpression;
        }

        private bool IsNonComposedSetOperation(SelectExpression selectExpression)
            => selectExpression.Offset == null
            && selectExpression.Limit == null
            && !selectExpression.IsDistinct
            && selectExpression.Predicate == null
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Tables.Count == 1
            && selectExpression.Tables[0] is SetOperationBase setOperation
            && selectExpression.Projection.Count == setOperation.Source1.Projection.Count
            && selectExpression.Projection.Select(
                (pe, index) => pe.Expression is ColumnExpression column
                    && string.Equals(column.Table.Alias, setOperation.Alias, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        column.Name, setOperation.Source1.Projection[index].Alias, StringComparison.OrdinalIgnoreCase))
            .All(e => e);

        protected virtual void GenerateSetOperation(SetOperationBase setOperation)
        {
            string getSetOperation() => setOperation switch
            {
                ExceptExpression _ => "EXCEPT",
                IntersectExpression _ => "INTERSECT",
                UnionExpression _ => "UNION",
                _ => throw new InvalidOperationException("Unknown SetOperationType."),
            };

            GenerateSetOperationOperand(setOperation, setOperation.Source1);
            _relationalCommandBuilder.AppendLine();
            _relationalCommandBuilder.AppendLine($"{getSetOperation()}{(setOperation.IsDistinct ? "" : " ALL")}");
            GenerateSetOperationOperand(setOperation, setOperation.Source2);
        }

        protected virtual void GenerateSetOperationOperand(SetOperationBase setOperation, SelectExpression operand)
        {
            // INTERSECT has higher precedence over UNION and EXCEPT, but otherwise evaluation is left-to-right.
            // To preserve meaning, add parentheses whenever a set operation is nested within a different set operation.
            if (IsNonComposedSetOperation(operand)
                && operand.Tables[0].GetType() != setOperation.GetType())
            {
                _relationalCommandBuilder.AppendLine("(");
                using (_relationalCommandBuilder.Indent())
                {
                    Visit(operand);
                }

                _relationalCommandBuilder.AppendLine().Append(")");
            }
            else
            {
                Visit(operand);
            }
        }

        protected virtual void GenerateTop(SelectExpression selectExpression)
        {
        }

        private void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                generationAction(items[i]);
            }
        }

        protected virtual void GenerateOrderings(SelectExpression selectExpression)
        {
            if (selectExpression.Orderings.Any())
            {
                var orderings = selectExpression.Orderings.ToList();

                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    orderings.RemoveAll(oe => oe.Expression is SqlConstantExpression || oe.Expression is SqlParameterExpression);
                }

                if (orderings.Count > 0)
                {
                    _relationalCommandBuilder.AppendLine()
                        .Append("ORDER BY ");

                    GenerateList(orderings, e => Visit(e));
                }
            }
            else if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine().Append("ORDER BY (SELECT 1)");
            }
        }

        protected virtual void GenerateLimitOffset(SelectExpression selectExpression)
        {
            if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);

                _relationalCommandBuilder.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    _relationalCommandBuilder.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    _relationalCommandBuilder.Append(" ROWS ONLY");
                }
            }
            else if (selectExpression.Limit != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("FETCH FIRST ");

                Visit(selectExpression.Limit);

                _relationalCommandBuilder.Append(" ROWS ONLY");
            }
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            _relationalCommandBuilder.Append("CROSS JOIN ");
            Visit(crossJoinExpression.Table);

            return crossJoinExpression;
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            _relationalCommandBuilder.Append("CROSS APPLY ");
            Visit(crossApplyExpression.Table);

            return crossApplyExpression;
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            _relationalCommandBuilder.Append("OUTER APPLY ");
            Visit(outerApplyExpression.Table);

            return outerApplyExpression;
        }

        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            _relationalCommandBuilder.Append("INNER JOIN ");
            Visit(innerJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(innerJoinExpression.JoinPredicate);

            return innerJoinExpression;
        }

        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            _relationalCommandBuilder.Append("LEFT JOIN ");
            Visit(leftJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(leftJoinExpression.JoinPredicate);

            return leftJoinExpression;
        }

        protected Expression VisitSubSelect(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                Visit(scalarSubqueryExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return scalarSubqueryExpression;
        }

        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            _relationalCommandBuilder.Append("ROW_NUMBER() OVER(");
            if (rowNumberExpression.Partitions.Any())
            {
                _relationalCommandBuilder.Append("PARTITION BY ");
                GenerateList(rowNumberExpression.Partitions, e => Visit(e));
                _relationalCommandBuilder.Append(" ");
            }

            _relationalCommandBuilder.Append("ORDER BY ");
            GenerateList(rowNumberExpression.Orderings, e => Visit(e));
            _relationalCommandBuilder.Append(")");

            return rowNumberExpression;
        }

        private void GenerateSetOperationHelper(SetOperationBase setOperation)
        {
            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                this.GenerateSetOperation(setOperation);
            }

            _relationalCommandBuilder.AppendLine()
                .Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(setOperation.Alias));
        }

        protected Expression VisitExcept(ExceptExpression exceptExpression)
        {
            GenerateSetOperationHelper(exceptExpression);

            return exceptExpression;
        }

        protected Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            GenerateSetOperationHelper(intersectExpression);

            return intersectExpression;
        }

        protected Expression VisitUnion(UnionExpression unionExpression)
        {
            GenerateSetOperationHelper(unionExpression);

            return unionExpression;
        }


    }
}
