using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    internal class QuerySqlGeneratorFactory : Microsoft.EntityFrameworkCore.Query.Internal.QuerySqlGeneratorFactory, IQuerySqlGeneratorFactoryWithCrossDb
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
