using Microsoft.EntityFrameworkCore.Metadata;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal interface ISqlExpressionFactoryWithCrossDb : Microsoft.EntityFrameworkCore.Query.ISqlExpressionFactory
    {
        SelectExpression SelectWithCrossDb(IEntityType entityType);
    }
}
