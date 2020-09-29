using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    public interface IQuerySqlGeneratorFactoryWithCrossDb : Microsoft.EntityFrameworkCore.Query.IQuerySqlGeneratorFactory
    {
        QuerySqlGeneratorWithCrossDb CreateWithCrossDb();
    }
}
