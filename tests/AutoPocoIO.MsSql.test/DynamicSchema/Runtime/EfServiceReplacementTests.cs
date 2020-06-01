using AutoPocoIO.MsSql.DynamicSchema.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using Moq;

namespace AutoPocoIO.MsSql.test.DynamicSchema.Runtime
{
    
    [Trait("Category", TestCategories.Unit)]
    public class EfServiceReplacementTests
    {

        [FactWithName]
        public void VerifyEfSqlCachingREmoved()
        {
            var optionBuilder = new Mock<DbContextOptionsBuilder>();
            optionBuilder.Setup(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>());

            optionBuilder.Object.ReplaceSqlServerEFCachingServices();

            optionBuilder.Verify(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>(), Times.Exactly(1));
            optionBuilder.Verify(c => c.ReplaceService<IRelationalTypeMappingSource, AutoPocoIO.DynamicSchema.Services.NoCache.SqlServerTypeMappingSource>(), Times.Once);
        }

        [FactWithName]
        public void VerifyEfCrossDb()
        {
            var optionBuilder = new Mock<DbContextOptionsBuilder>();
            optionBuilder.Setup(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>());

            optionBuilder.Object.ReplaceSqlServerEFCrossDbServices();

            optionBuilder.Verify(c => c.ReplaceService<It.IsAnyType, It.IsAnyType>(), Times.Exactly(2));
            optionBuilder.Verify(c => c.ReplaceService<IQuerySqlGeneratorFactory, AutoPocoIO.DynamicSchema.Services.CrossDb.SqlServerQuerySqlGeneratorFactory>(), Times.Once);
            optionBuilder.Verify(c => c.ReplaceService<IModelValidator, AutoPocoIO.DynamicSchema.Services.CrossDb.SqlServerModelValidator>(), Times.Once);
        }
    }
}
