using AutoPocoIO.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoPocoIO.DynamicSchema.Runtime
{
    /// <summary>
    /// Entity framework service replacements.
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// Remove services that cache database models.
        /// </summary>
        /// <param name="optionBuilder">Context builder for model.</param>
        /// <returns>DbContextOptionsBuilder to chain calls.</returns>
        public static DbContextOptionsBuilder ReplaceEFCachingServices(this DbContextOptionsBuilder optionBuilder)
        {
            Check.NotNull(optionBuilder, nameof(optionBuilder));

            optionBuilder.ReplaceService<IModelSource, Services.NoCache.ModelSource>();
            optionBuilder.ReplaceService<IDbSetFinder, Services.NoCache.DbSetFinder>();
            optionBuilder.ReplaceService<IDbSetSource, Services.NoCache.DbSetSource>();

#if NETFULL || NETCORE2_2
            optionBuilder.ReplaceService<IDbQuerySource, Services.NoCache.DbSetSource>();
#endif
            optionBuilder.ReplaceService<ICompiledQueryCache, Services.NoCache.CompiledQueryCache>();
            optionBuilder.ReplaceService<IEntityFinderSource, Services.NoCache.EntityFinderSource>();
            optionBuilder.ReplaceService<IRelationalValueBufferFactoryFactory, Services.NoCache.TypedRelationalValueBufferFactoryFactory>();

            return optionBuilder;
        }

        /// <summary>
        /// Add same server, cross database access
        /// </summary>
        /// <param name="optionBuilder">Context builder for model.</param>
        /// <returns>DbContextOptionsBuilder to chain calls.</returns>
        public static DbContextOptionsBuilder ReplaceEFCrossDbServices(this DbContextOptionsBuilder optionBuilder)
        {
            Check.NotNull(optionBuilder, nameof(optionBuilder));

#if NETCORE3_1
            optionBuilder.ReplaceService<ISqlExpressionFactory, Services.CrossDb.SqlExpressionFactory>();
#else
            optionBuilder.ReplaceService<Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.IEntityQueryableExpressionVisitorFactory, Services.CrossDb.RelationalEntityQueryableExpressionVisitorFactory>();
            optionBuilder.ReplaceService<Microsoft.EntityFrameworkCore.Query.Expressions.ISelectExpressionFactory, Services.CrossDb.SelectExpressionFactory>();
            optionBuilder.ReplaceService<IEntityQueryModelVisitorFactory, Services.CrossDb.RelationalQueryModelVisitorFactory>();
#endif
            return optionBuilder;

        }
    }
}