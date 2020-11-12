using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal interface ISqlExpressionFactoryWithCrossDb : Microsoft.EntityFrameworkCore.Query.ISqlExpressionFactory
    {
        SelectExpression SelectWithCrossDb(IEntityType entityType);
        ExistsExpression Exists(SelectExpression subquery, bool negated);
        InExpression In(SqlExpression item, SelectExpression subquery, bool negated);
    }
}
