using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;
using System.Linq;

namespace AutoPocoIO.MsSql.test.Migrations
{
    
    [Trait("Category", TestCategories.Unit)]
    public class AppDbSnapShotTests
    {
        private class AppDBSnapShot : AppDbContextModelSnapshot
        {
            public void BuildModelPublic(ModelBuilder modelBuilder) => base.BuildModel(modelBuilder);
        }

        private readonly AppDBSnapShot snapShot;
        public AppDbSnapShotTests()
        {
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);

            snapShot = new AppDBSnapShot();
            snapShot.BuildModelPublic(builder);
        }

        [FactWithName]
        public void BuilderHasAnnotations()
        {
            Assert.Equal("2.2.6-servicing-10079", snapShot.Model.FindAnnotation("ProductVersion").Value);
            Assert.Equal(128, snapShot.Model.FindAnnotation("Relational:MaxIdentifierLength").Value);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, snapShot.Model.FindAnnotation("SqlServer:ValueGenerationStrategy").Value);
        }

        [FactWithName]
        public void ConnectorTableSetUp()
        {
            var entity = snapShot.Model.FindEntityType("AutoPoco.Models.Connector");

            //Table Metadata
            Assert.Equal("AutoPoco", entity.Relational().Schema);
            Assert.Equal("Connector", entity.Relational().TableName);

            //PK
            Assert.Equal(entity.FindProperty("Id"), entity.FindPrimaryKey().Properties.Single());

            //FK
            var fks = entity.GetNavigations();
            Assert.Empty(fks);

            //Index
            Assert.True(entity.GetIndexes().First(c => c.Relational().Name == "IDX_ConnectorName").IsUnique);
            Assert.Equal("[Name] IS NOT NULL", entity.GetIndexes().First(c => c.Relational().Name == "IDX_ConnectorName").Relational().Filter);

            //Columns (if column not found then will throw nullobject)
            Assert.Equal(11, entity.GetProperties().Count());

            Assert.Equal(typeof(int), entity.FindProperty("Id").ClrType);
            Assert.Equal(ValueGenerated.OnAdd, entity.FindProperty("Id").ValueGenerated);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, entity.FindProperty("Id").FindAnnotation("SqlServer:ValueGenerationStrategy").Value);

            Assert.Equal(typeof(string), entity.FindProperty("ConnectionString").ClrType);
            Assert.Equal(typeof(string), entity.FindProperty("DataSource").ClrType);
            Assert.Equal(500, entity.FindProperty("DataSource").GetMaxLength());

            Assert.Equal(typeof(string), entity.FindProperty("InitialCatalog").ClrType);
            Assert.Equal(50, entity.FindProperty("InitialCatalog").GetMaxLength());

            Assert.Equal(typeof(int), entity.FindProperty("RecordLimit").ClrType);
            Assert.Equal(typeof(int), entity.FindProperty("ResourceType").ClrType);
            Assert.Equal(typeof(string), entity.FindProperty("Schema").ClrType);
            Assert.Equal(50, entity.FindProperty("Schema").GetMaxLength());

            Assert.Equal(typeof(string), entity.FindProperty("UserId").ClrType);
            Assert.Equal(50, entity.FindProperty("UserId").GetMaxLength());

            Assert.Equal(typeof(int), entity.FindProperty("Port").ClrType);
            Assert.Equal(typeof(bool), entity.FindProperty("IsActive").ClrType);
        }

        [FactWithName]
        public void UserJoinTableSetUp()
        {
            var entity = snapShot.Model.FindEntityType("AutoPoco.Models.UserJoin");

            //Table Metadata
            Assert.Equal("AutoPoco", entity.Relational().Schema);
            Assert.Equal("UserJoin", entity.Relational().TableName);

            //PK
            Assert.Equal(entity.FindProperty("Id"), entity.FindPrimaryKey().Properties.Single());

            //FK
            var fks = entity.GetNavigations();
            Assert.Equal(2, fks.Count());

            var connectorModel = snapShot.Model.FindEntityType("AutoPoco.Models.Connector");
            Assert.Equal(connectorModel.FindProperty("Id"), fks.First(c => c.Name == "FKConnector").ForeignKey.PrincipalKey.Properties.Single());
            Assert.Equal(entity.FindProperty("FKConnectorId"), fks.First(c => c.Name == "FKConnector").ForeignKey.Properties.Single());

            Assert.Equal(connectorModel.FindProperty("Id"), fks.First(c => c.Name == "PKConnector").ForeignKey.PrincipalKey.Properties.Single());
            Assert.Equal(entity.FindProperty("PKConnectorId"), fks.First(c => c.Name == "PKConnector").ForeignKey.Properties.Single());


            //Index
            Assert.True(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_Alias").IsUnique);
            Assert.False(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_FKConnectorId").IsUnique);
            Assert.False(entity.GetIndexes().First(c => c.Relational().Name == "IX_UserJoin_PKConnectorId").IsUnique);


            //Columns (if column not found then will throw nullobject)
            Assert.Equal(8, entity.GetProperties().Count());

            Assert.Equal(typeof(int), entity.FindProperty("Id").ClrType);
            Assert.Equal(ValueGenerated.OnAdd, entity.FindProperty("Id").ValueGenerated);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, entity.FindProperty("Id").FindAnnotation("SqlServer:ValueGenerationStrategy").Value);

            Assert.Equal(typeof(string), entity.FindProperty("Alias").ClrType);
            Assert.Equal(50, entity.FindProperty("Alias").GetMaxLength());
            Assert.False(entity.FindProperty("Alias").IsColumnNullable());

            Assert.Equal(typeof(string), entity.FindProperty("FKColumn").ClrType);
            Assert.Equal(500, entity.FindProperty("FKColumn").GetMaxLength());
            Assert.False(entity.FindProperty("FKColumn").IsColumnNullable());

            Assert.Equal(typeof(int?), entity.FindProperty("FKConnectorId").ClrType);

            Assert.Equal(typeof(string), entity.FindProperty("FKTableName").ClrType);
            Assert.Equal(100, entity.FindProperty("FKTableName").GetMaxLength());
            Assert.False(entity.FindProperty("FKTableName").IsColumnNullable());

            Assert.Equal(typeof(string), entity.FindProperty("PKColumn").ClrType);
            Assert.Equal(500, entity.FindProperty("PKColumn").GetMaxLength());
            Assert.False(entity.FindProperty("PKColumn").IsColumnNullable());

            Assert.Equal(typeof(int?), entity.FindProperty("PKConnectorId").ClrType);

            Assert.Equal(typeof(string), entity.FindProperty("PKTableName").ClrType);
            Assert.Equal(100, entity.FindProperty("PKTableName").GetMaxLength());
            Assert.False(entity.FindProperty("PKTableName").IsColumnNullable());
        }
    }
}
