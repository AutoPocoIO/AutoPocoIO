using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    public class QuerySqlGeneratorWithCrossDb : Microsoft.EntityFrameworkCore.Query.QuerySqlGenerator
    {

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

        protected Expression VisitColumn(ColumnExpression columnExpression)
        {
            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Table.Alias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
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

    }
}
