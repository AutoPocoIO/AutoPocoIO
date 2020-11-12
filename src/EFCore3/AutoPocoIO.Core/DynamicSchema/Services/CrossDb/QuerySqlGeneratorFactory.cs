using Microsoft.EntityFrameworkCore.Query;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class QuerySqlGeneratorFactory : Microsoft.EntityFrameworkCore.Query.Internal.QuerySqlGeneratorFactory, IQuerySqlGeneratorFactoryWithCrossDb
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;

        public QuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies) : base(dependencies)
        {
            _dependencies = dependencies;
        }

        public virtual QuerySqlGeneratorWithCrossDb CreateWithCrossDb()
            => new QuerySqlGeneratorWithCrossDb(_dependencies);

    }
}
