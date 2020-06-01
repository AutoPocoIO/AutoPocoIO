using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoPocoIO.test.Migrations
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class AppDbSnapShotTests
    {
        private class AppDBSnapShot : AppDbContextModelSnapshot
        {
            public void BuildModelPublic(ModelBuilder modelBuilder) => base.BuildModel(modelBuilder);
        }

        private AppDBSnapShot snapShot;

        [TestInitialize]
        public void Init()
        {
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);

            snapShot = new AppDBSnapShot();
            snapShot.BuildModelPublic(builder);
        }

        [TestMethod]
        public void BuilderHasAnnotations()
        {
            Assert.AreEqual("2.2.6-servicing-10079", snapShot.Model.FindAnnotation("ProductVersion").Value);
            Assert.AreEqual(128, snapShot.Model.FindAnnotation("Relational:MaxIdentifierLength").Value);
            Assert.AreEqual(SqlServerValueGenerationStrategy.IdentityColumn, snapShot.Model.FindAnnotation("SqlServer:ValueGenerationStrategy").Value);
        }

        [TestMethod]
        public void ConnectorTableSetUp()
        {
            var entity = snapShot.Model.FindEntityType("AutoPoco.Models.Connector");

            //Table Metadata
            Assert.AreEqual("AutoPoco", entity.Relational().Schema);
            Assert.AreEqual("Connector", entity.Relational().TableName);

            //PK
            Assert.AreEqual(entity.FindProperty("Id"), entity.FindPrimaryKey().Properties.Single());

            //FK
            var fks = entity.GetNavigations();
            Assert.AreEqual(0, fks.Count());

            //Index
            Assert.IsTrue(entity.GetIndexes().First(c => c.Relational().Name == "IDX_ConnectorName").IsUnique);
            Assert.AreEqual("[Name] IS NOT NULL", entity.GetIndexes().First(c => c.Relational().Name == "IDX_ConnectorName").Relational().Filter);

            //Columns (if column not found then will throw nullobject)
            Assert.AreEqual(11, entity.GetProperties().Count());

            Assert.AreEqual(typeof(int), entity.FindProperty("Id").ClrType);
            Assert.AreEqual(ValueGenerated.OnAdd, entity.FindProperty("Id").ValueGenerated);
            Assert.AreEqual(SqlServerValueGenerationStrategy.IdentityColumn, entity.FindProperty("Id").FindAnnotation("SqlServer:ValueGenerationStrategy").Value);

            Assert.AreEqual(typeof(string), entity.FindProperty("ConnectionString").ClrType);
            Assert.AreEqual(typeof(string), entity.FindProperty("DataSource").ClrType);
            Assert.AreEqual(500, entity.FindProperty("DataSource").GetMaxLength());

            Assert.AreEqual(typeof(string), entity.FindProperty("InitialCatalog").ClrType);
            Assert.AreEqual(50, entity.FindProperty("InitialCatalog").GetMaxLength());

            Assert.AreEqual(typeof(int), entity.FindProperty("RecordLimit").ClrType);
            Assert.AreEqual(typeof(int), entity.FindProperty("ResourceType").ClrType);
            Assert.AreEqual(typeof(string), entity.FindProperty("Schema").ClrType);
            Assert.AreEqual(50, entity.FindProperty("Schema").GetMaxLength());

            Assert.AreEqual(typeof(string), entity.FindProperty("UserId").ClrType);
            Assert.AreEqual(50, entity.FindProperty("UserId").GetMaxLength());

            Assert.AreEqual(typeof(int), entity.FindProperty("Port").ClrType);
            Assert.AreEqual(typeof(bool), entity.FindProperty("IsActive").ClrType);
        }

        [TestMethod]
        public void UserJoinTableSetUp()
        {
            var entity = snapShot.Model.FindEntityType("AutoPoco.Models.UserJoin");

            //Table Metadata
            Assert.AreEqual("AutoPoco", entity.Relational().Schema);
            Assert.AreEqual("UserJoin", entity.Relational().TableName);

            //PK
            Assert.AreEqual(entity.FindProperty("Id"), entity.FindPrimaryKey().Properties.Single());

            //FK
            var fks = entity.GetNavigations();
            Assert.AreEqual(2, fks.Count());

            var connectorModel = snapShot.Model.FindEntityType("AutoPoco.Models.Connector");
            Assert.AreEqual(connectorModel.FindProperty("Id"), fks.First(c => c.Name == "FKConnector").ForeignKey.PrincipalKey.Properties.Single());
            Assert.AreEqual(entity.FindProperty("FKConnectorId"), fks.First(c => c.Name == "FKConnector").ForeignKey.Properties.Single());

            Assert.AreEqual(connectorModel.FindProperty("Id"), fks.First(c => c.Name == "PKConnector").ForeignKey.PrincipalKey.Properties.Single());
            Assert.AreEqual(entity.FindProperty("PKConnectorId"), fks.First(c => c.Name == "PKConnector").ForeignKey.Properties.Single());


            //Index
            Assert.IsTrue(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_Alias").IsUnique);
            Assert.IsFalse(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_FKConnectorId").IsUnique);
            Assert.IsFalse(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_PKConnectorId").IsUnique);


            //Columns (if column not found then will throw nullobject)
            Assert.AreEqual(8, entity.GetProperties().Count());

            Assert.AreEqual(typeof(int), entity.FindProperty("Id").ClrType);
            Assert.AreEqual(ValueGenerated.OnAdd, entity.FindProperty("Id").ValueGenerated);
            Assert.AreEqual(SqlServerValueGenerationStrategy.IdentityColumn, entity.FindProperty("Id").FindAnnotation("SqlServer:ValueGenerationStrategy").Value);

            Assert.AreEqual(typeof(string), entity.FindProperty("Alias").ClrType);
            Assert.AreEqual(50, entity.FindProperty("Alias").GetMaxLength());
            Assert.IsFalse(entity.FindProperty("Alias").IsColumnNullable());

            Assert.AreEqual(typeof(string), entity.FindProperty("FKColumn").ClrType);
            Assert.AreEqual(500, entity.FindProperty("FKColumn").GetMaxLength());
            Assert.IsFalse(entity.FindProperty("FKColumn").IsColumnNullable());

            Assert.AreEqual(typeof(int?), entity.FindProperty("FKConnectorId").ClrType);

            Assert.AreEqual(typeof(string), entity.FindProperty("FKTableName").ClrType);
            Assert.AreEqual(100, entity.FindProperty("FKTableName").GetMaxLength());
            Assert.IsFalse(entity.FindProperty("FKTableName").IsColumnNullable());

            Assert.AreEqual(typeof(string), entity.FindProperty("PKColumn").ClrType);
            Assert.AreEqual(500, entity.FindProperty("PKColumn").GetMaxLength());
            Assert.IsFalse(entity.FindProperty("PKColumn").IsColumnNullable());

            Assert.AreEqual(typeof(int?), entity.FindProperty("PKConnectorId").ClrType);

            Assert.AreEqual(typeof(string), entity.FindProperty("PKTableName").ClrType);
            Assert.AreEqual(100, entity.FindProperty("PKTableName").GetMaxLength());
            Assert.IsFalse(entity.FindProperty("PKTableName").IsColumnNullable());
        }
    }
}
