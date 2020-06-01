using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
    public class ModelBuilderExtensionTests
    {
        [FactWithName]
        public void CreateModelAppliesConnectorAndUserJoinConfigs()
        {
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);

            builder.CreateModel();

            var entity = builder.Model.FindEntityType("AutoPocoIO.Models.Connector");
             Assert.True(entity.GetIndexes().First(c => c.Relational().Name == "IDX_ConnectorName").IsUnique);

            entity = builder.Model.FindEntityType("AutoPocoIO.Models.UserJoin");
             Assert.True(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_Alias").IsUnique);
        }

        [FactWithName]
        public void SeedConnectorData()
        {
            Connector[] data = null;
            var connectorBuilder = MockEntityTypeBuilder<Connector>();
            var joinBuilder = MockEntityTypeBuilder<UserJoin>();
            connectorBuilder.Setup(c => c.HasData(It.IsAny<Connector[]>()))
                .Callback<Connector[]>(c => data = c);

            var builder = new Mock<ModelBuilder>(new ConventionSet());
            builder.Setup(c => c.Entity<Connector>()).Returns(connectorBuilder.Object);
            builder.Setup(c => c.Entity<UserJoin>()).Returns(joinBuilder.Object);

            builder.Object.Seed();

            Assert.Equal(2, data.Length);

            Assert.Equal(1, data[0].Id);
            Assert.Equal("appDb", data[0].Name);
            Assert.Equal(1, data[0].ResourceType);
            Assert.Equal("", data[0].ConnectionString);
            Assert.Equal("AutoPoco", data[0].Schema);
            Assert.Equal(500, data[0].RecordLimit);

            Assert.Equal(2, data[1].Id);
            Assert.Equal("logDb", data[1].Name);
            Assert.Equal(1, data[1].ResourceType);
            Assert.Equal("", data[1].ConnectionString);
            Assert.Equal("AutoPocoLog", data[1].Schema);
            Assert.Equal(500, data[1].RecordLimit);
        }

        [FactWithName]
        public void SeedUserJoinData()
        {
            UserJoin[] data = null;

            var connectorBuilder = MockEntityTypeBuilder<Connector>();
            var joinBuilder = MockEntityTypeBuilder<UserJoin>();
            joinBuilder.Setup(c => c.HasData(It.IsAny<UserJoin[]>()))
             .Callback<UserJoin[]>(c => data = c);

            var builder = new Mock<ModelBuilder>(new ConventionSet());
            builder.Setup(c => c.Entity<Connector>())
                .Returns(connectorBuilder.Object);

            builder.Setup(c => c.Entity<UserJoin>())
              .Returns(joinBuilder.Object);

            builder.Object.Seed();

            Assert.Single(data);

            Assert.Equal(1, data[0].Id);
            Assert.Equal("Response", data[0].Alias);
            Assert.Equal(2, data[0].PKConnectorId);
            Assert.Equal(2, data[0].FKConnectorId);
            Assert.Equal("Request", data[0].PKTableName);
            Assert.Equal("RequestId,RequestGuid", data[0].PKColumn);
            Assert.Equal("Response", data[0].FKTableName);
            Assert.Equal("ResponseId,RequestGuid", data[0].FKColumn);
        }

        private Mock<EntityTypeBuilder<T>> MockEntityTypeBuilder<T>() where T : class
        {
            Model model = new Model();
            EntityType entityType = new EntityType("a", model, ConfigurationSource.Explicit);
            InternalModelBuilder internalModelBuilder = new InternalModelBuilder(model);
            InternalEntityTypeBuilder internalEntityTypeBuilder = new InternalEntityTypeBuilder(entityType, internalModelBuilder);

            return new Mock<EntityTypeBuilder<T>>(internalEntityTypeBuilder);
        }
    }
}
