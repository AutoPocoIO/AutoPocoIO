using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ModelBuilderExtensionTests
    {
        [TestMethod]
        public void CreateModelAppliesConnectorAndUserJoinConfigs()
        {
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);

            builder.CreateModel();

            var entity = builder.Model.FindEntityType("AutoPocoIO.Models.Connector");
            Assert.IsTrue(entity.GetIndexes().First(c => c.Relational().Name == "IDX_ConnectorName").IsUnique);

            entity = builder.Model.FindEntityType("AutoPocoIO.Models.UserJoin");
            Assert.IsTrue(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_Alias").IsUnique);
        }

        [TestMethod]
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

            Assert.AreEqual(2, data.Length);

            Assert.AreEqual(Guid.Parse("4b6b6ba7-0209-4b89-91cb-0e2a67aa37c1"), data[0].Id);
            Assert.AreEqual("appDb", data[0].Name);
            Assert.AreEqual("", data[0].ResourceType);
            Assert.AreEqual("", data[0].ConnectionString);
            Assert.AreEqual("AutoPoco", data[0].Schema);
            Assert.AreEqual(500, data[0].RecordLimit);

            Assert.AreEqual(Guid.Parse("4d74e770-54e9-4b0f-8f13-59ccb0808654"), data[1].Id);
            Assert.AreEqual("logDb", data[1].Name);
            Assert.AreEqual("", data[1].ResourceType);
            Assert.AreEqual("", data[1].ConnectionString);
            Assert.AreEqual("AutoPocoLog", data[1].Schema);
            Assert.AreEqual(500, data[1].RecordLimit);
        }

        [TestMethod]
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

            Assert.AreEqual(1, data.Length);

            Assert.AreEqual(Guid.Parse("abd7f037-cc34-44fb-89f5-2e4a06772a01"), data[0].Id);
            Assert.AreEqual("Response", data[0].Alias);
            Assert.AreEqual(Guid.Parse("4d74e770-54e9-4b0f-8f13-59ccb0808654"), data[0].PKConnectorId);
            Assert.AreEqual(Guid.Parse("4d74e770-54e9-4b0f-8f13-59ccb0808654"), data[0].FKConnectorId);
            Assert.AreEqual("Request", data[0].PKTableName);
            Assert.AreEqual("RequestId,RequestGuid", data[0].PKColumn);
            Assert.AreEqual("Response", data[0].FKTableName);
            Assert.AreEqual("ResponseId,RequestGuid", data[0].FKColumn);
        }


#if EF31
        private Mock<EntityTypeBuilder<T>> MockEntityTypeBuilder<T>() where T : class
        {
            Model model = new Model();
            IMutableEntityType entityType = new EntityType("a", model, ConfigurationSource.Explicit);

            return new Mock<EntityTypeBuilder<T>>(entityType);
        }
#else
        private Mock<EntityTypeBuilder<T>> MockEntityTypeBuilder<T>() where T : class
        {
            Model model = new Model();
            EntityType entityType = new EntityType("a", model, ConfigurationSource.Explicit);
            InternalModelBuilder internalModelBuilder = new InternalModelBuilder(model);
            InternalEntityTypeBuilder internalEntityTypeBuilder = new InternalEntityTypeBuilder(entityType, internalModelBuilder);

            return new Mock<EntityTypeBuilder<T>>(internalEntityTypeBuilder);
        }
#endif
    }
}
