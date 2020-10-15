namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal interface IQuerySqlGeneratorFactoryWithCrossDb : Microsoft.EntityFrameworkCore.Query.IQuerySqlGeneratorFactory
    {
        QuerySqlGeneratorWithCrossDb CreateWithCrossDb();
    }
}
