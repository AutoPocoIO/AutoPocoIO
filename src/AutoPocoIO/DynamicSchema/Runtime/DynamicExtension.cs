using AutoPocoIO.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoPocoIO.DynamicSchema.Runtime
{
    public static class DynamicExtensions
    {
        public static DbContextOptionsBuilder ReplaceEFCachingServices(this DbContextOptionsBuilder optionBuilder)
        {
            Check.NotNull(optionBuilder, nameof(optionBuilder));

            optionBuilder.ReplaceService<IModelSource, Services.NoCache.ModelSource>();
            optionBuilder.ReplaceService<IDbSetFinder, Services.NoCache.DbSetFinder>();
            optionBuilder.ReplaceService<IDbSetSource, Services.NoCache.DbSetSource>();
            optionBuilder.ReplaceService<IDbQuerySource, Services.NoCache.DbSetSource>();
            optionBuilder.ReplaceService<ICompiledQueryCache, Services.NoCache.CompiledQueryCache>();
            optionBuilder.ReplaceService<IEntityFinderSource, Services.NoCache.EntityFinderSource>();
            optionBuilder.ReplaceService<IRelationalValueBufferFactoryFactory, Services.NoCache.TypedRelationalValueBufferFactoryFactory>();

            return optionBuilder;
        }

        public static DbContextOptionsBuilder ReplaceEFCrossDbServices(this DbContextOptionsBuilder optionBuilder)
        {
            Check.NotNull(optionBuilder, nameof(optionBuilder));
            optionBuilder.ReplaceService<IEntityQueryableExpressionVisitorFactory, Services.CrossDb.RelationalEntityQueryableExpressionVisitorFactory>();
            optionBuilder.ReplaceService<ISelectExpressionFactory, Services.CrossDb.SelectExpressionFactory>();
            optionBuilder.ReplaceService<IEntityQueryModelVisitorFactory, Services.CrossDb.RelationalQueryModelVisitorFactory>();
            return optionBuilder;

        }
    }
}