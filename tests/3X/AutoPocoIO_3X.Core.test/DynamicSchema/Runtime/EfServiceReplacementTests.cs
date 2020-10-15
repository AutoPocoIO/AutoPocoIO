using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Runtime;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

#if EF22
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
#endif

namespace AutoPocoIO.test.DynamicSchema.Runtime
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class EfServiceReplacementTests
    {
        [TestMethod]
        public void VerifyEfCachingRemoved()
        {
            var optionBuilder = new Mock<DbContextOptionsBuilder>();
            optionBuilder.Setup(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>());

            optionBuilder.Object.ReplaceEFCachingServices();

            optionBuilder.Verify(c => c.ReplaceService<IModelSource, AutoPocoIO.DynamicSchema.Services.NoCache.ModelSource>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IDbSetFinder, AutoPocoIO.DynamicSchema.Services.NoCache.DbSetFinder>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IDbSetSource, AutoPocoIO.DynamicSchema.Services.NoCache.DbSetSource>(), Times.Once);
#if EF22
            optionBuilder.Verify(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>(), Times.Exactly(7));
            optionBuilder.Verify(c => c.ReplaceService<IDbQuerySource, AutoPocoIO.DynamicSchema.Services.NoCache.DbSetSource>(), Times.Once);
#else
            optionBuilder.Verify(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>(), Times.Exactly(6));
#endif
            optionBuilder.Verify(c => c.ReplaceService<ICompiledQueryCache, AutoPocoIO.DynamicSchema.Services.NoCache.CompiledQueryCache>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IEntityFinderSource, AutoPocoIO.DynamicSchema.Services.NoCache.EntityFinderSource>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IRelationalValueBufferFactoryFactory, AutoPocoIO.DynamicSchema.Services.NoCache.TypedRelationalValueBufferFactoryFactory>(), Times.Once);
        }

        [TestMethod]
        public void VerifyEfCrossDb()
        {
            var optionBuilder = new Mock<DbContextOptionsBuilder>();
            optionBuilder.Setup(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>());

            optionBuilder.Object.ReplaceEFCrossDbServices();

#if EF22
            optionBuilder.Verify(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>(), Times.Exactly(3));
            optionBuilder.Verify(c => c.ReplaceService<IEntityQueryableExpressionVisitorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.RelationalEntityQueryableExpressionVisitorFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<ISelectExpressionFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.SelectExpressionFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IEntityQueryModelVisitorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.RelationalQueryModelVisitorFactory>(), Times.Once);
#else
            optionBuilder.Verify(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>(), Times.Exactly(7));
            optionBuilder.Verify(c => c.ReplaceService<ISqlExpressionFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.SqlExpressionFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IQueryableMethodTranslatingExpressionVisitorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.RelationalQueryableMethodTranslatingExpressionVisitorFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<AutoPocoIO.DynamicSchema.Services.CrossDb.IQuerySqlGeneratorFactoryWithCrossDb, AutoPocoIO.DynamicSchema.Services.CrossDb.QuerySqlGeneratorFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IQuerySqlGeneratorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.QuerySqlGeneratorFactory>(), Times.Exactly(2));
            optionBuilder.Verify(c => c.ReplaceService<IShapedQueryCompilingExpressionVisitorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.RelationalShapedQueryCompilingExpressionVisitorFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IQueryTranslationPostprocessorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.RelationalQueryTranslationPostprocessorFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IRelationalSqlTranslatingExpressionVisitorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.RelationalSqlTranslatingExpressionVisitorFactory>(), Times.Once);
#endif
        }


        //private class context1 : DbContext
        //{
        //    public context1(DbContextOptions options) : base(options)
        //    {
        //    }

        //    public DbSet<Connector> Connectors {get;set;}
        //}

        //[TestMethod]
        //public void CheckTakeWorks()
        //{
        //    var appDbOptions = new DbContextOptionsBuilder()
        //         .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DataVault;Integrated Security=True;MultipleActiveResultSets=True;App=EntityFramework")
        //         .ReplaceEFCrossDbServices();

        //    var context = new context1(appDbOptions.Options);


        //    context.Connectors.Select(c => new { c.Name, c.Port })
        //        .Take(1)
        //        .ToList();

        //}
    }
}
