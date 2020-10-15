namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    public interface IQuerySqlGeneratorFactoryWithCrossDb : Microsoft.EntityFrameworkCore.Query.IQuerySqlGeneratorFactory
    {
        QuerySqlGeneratorWithCrossDb CreateWithCrossDb();
    }
}
