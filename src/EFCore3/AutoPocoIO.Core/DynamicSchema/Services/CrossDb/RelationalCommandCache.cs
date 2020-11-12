
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class RelationalCommandCache : Microsoft.EntityFrameworkCore.Query.Internal.RelationalCommandCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IQuerySqlGeneratorFactoryWithCrossDb _querySqlGeneratorFactory;
        private readonly SelectExpression _selectExpression;
        private readonly ParameterValueBasedSelectExpressionOptimizer _parameterValueBasedSelectExpressionOptimizer;

        public RelationalCommandCache(
         IMemoryCache memoryCache,
         ISqlExpressionFactory sqlExpressionFactory,
         IParameterNameGeneratorFactory parameterNameGeneratorFactory,
         IQuerySqlGeneratorFactoryWithCrossDb querySqlGeneratorFactory,
         bool useRelationalNulls,
         SelectExpression selectExpression)
            : base(memoryCache, sqlExpressionFactory, parameterNameGeneratorFactory, querySqlGeneratorFactory, useRelationalNulls, null)
        {
            _memoryCache = memoryCache;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _selectExpression = selectExpression;

            _parameterValueBasedSelectExpressionOptimizer = new ParameterValueBasedSelectExpressionOptimizer(
                sqlExpressionFactory,
                parameterNameGeneratorFactory,
                useRelationalNulls);
        }

        public override IRelationalCommand GetRelationalCommand(IReadOnlyDictionary<string, object> parameters)
        {

            var (selectExpression, canCache) =
                _parameterValueBasedSelectExpressionOptimizer.Optimize(_selectExpression, parameters);
            var relationalCommand = _querySqlGeneratorFactory.CreateWithCrossDb().GetCommand(selectExpression);


            return relationalCommand;
        }

    }
}
