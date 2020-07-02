using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoPocoIO.test.Migrations
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class AppDbUpDownTests
    {
        private class AppDBMigration : AppDb
        {
            public void UpPublic(MigrationBuilder modelBuilder) => base.Up(modelBuilder);
            public void DownPublic(MigrationBuilder modelBuilder) => base.Down(modelBuilder);

        }

        private AppDBMigration migration;

        [TestInitialize]
        public void Init()
        {
            var builder = new MigrationBuilder("System.Data.SqlClient");

            migration = new AppDBMigration();
            migration.UpPublic(builder);
            migration.DownPublic(builder);
        }

        [TestMethod]
        public void EnsureAutoPocoSchema()
        {
            Assert.AreEqual("AutoPoco", ((EnsureSchemaOperation)migration.UpOperations[0]).Name);
        }

        [TestMethod]
        public void ConnectorTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "Connector");

            Assert.AreEqual("AutoPoco", op.Schema);

            //PK
            Assert.AreEqual("PK_Connector", op.PrimaryKey.Name);
            CollectionAssert.AreEqual(new[] { "Id" }, op.PrimaryKey.Columns);

            //FK
            Assert.AreEqual(0, op.ForeignKeys.Count());

            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "Connector").Cast<CreateIndexOperation>();
            Assert.AreEqual(1, idxOps.Count());

            CollectionAssert.AreEqual(new[] { "Name" }, idxOps.First(c => c.Name == "IDX_ConnectorName").Columns);
            Assert.AreEqual(true, idxOps.First(c => c.Name == "IDX_ConnectorName").IsUnique);
            Assert.AreEqual("[Name] IS NOT NULL", idxOps.First(c => c.Name == "IDX_ConnectorName").Filter);

            //Columns
            Assert.AreEqual(11, op.Columns.Count());

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Id").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "Id").IsNullable);
            Assert.AreEqual(128, op.Columns.First(c => c.Name == "Id").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Name").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "Name").IsNullable);
            Assert.AreEqual(50, op.Columns.First(c => c.Name == "Name").MaxLength);

            Assert.AreEqual(typeof(int), op.Columns.First(c => c.Name == "ResourceType").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "ResourceType").IsNullable);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Schema").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "Schema").IsNullable);
            Assert.AreEqual(50, op.Columns.First(c => c.Name == "Schema").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "ConnectionString").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "ConnectionString").IsNullable);

            Assert.AreEqual(typeof(int), op.Columns.First(c => c.Name == "RecordLimit").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "RecordLimit").IsNullable);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "UserId").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "UserId").IsNullable);
            Assert.AreEqual(50, op.Columns.First(c => c.Name == "UserId").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "InitialCatalog").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "InitialCatalog").IsNullable);
            Assert.AreEqual(50, op.Columns.First(c => c.Name == "InitialCatalog").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "DataSource").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "DataSource").IsNullable);
            Assert.AreEqual(500, op.Columns.First(c => c.Name == "DataSource").MaxLength);

            Assert.AreEqual(typeof(int), op.Columns.First(c => c.Name == "Port").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "Port").IsNullable);

            Assert.AreEqual(typeof(bool), op.Columns.First(c => c.Name == "IsActive").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "IsActive").IsNullable);

        }

        [TestMethod]
        public void UserJoinTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "UserJoin");

            Assert.AreEqual("AutoPoco", op.Schema);

            //PK
            Assert.AreEqual("PK_UserJoin", op.PrimaryKey.Name);
            CollectionAssert.AreEqual(new[] { "Id" }, op.PrimaryKey.Columns);

            //FK
            var fk = op.ForeignKeys.First(c => c.Name == "FK_UserJoin_Connector_FKConnectorId");
            CollectionAssert.AreEqual(new[] { "FKConnectorId" }, fk.Columns);
            Assert.AreEqual("AutoPoco.Connector.Id", $"{fk.PrincipalSchema}.{fk.PrincipalTable}.{fk.PrincipalColumns[0]}");
            Assert.AreEqual(ReferentialAction.Restrict, fk.OnDelete);

            fk = op.ForeignKeys.First(c => c.Name == "FK_UserJoin_Connector_PKConnectorId");
            CollectionAssert.AreEqual(new[] { "PKConnectorId" }, fk.Columns);
            Assert.AreEqual("AutoPoco.Connector.Id", $"{fk.PrincipalSchema}.{fk.PrincipalTable}.{fk.PrincipalColumns[0]}");
            Assert.AreEqual(ReferentialAction.Restrict, fk.OnDelete);


            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "UserJoin").Cast<CreateIndexOperation>();
            Assert.AreEqual(3, idxOps.Count());

            CollectionAssert.AreEqual(new[] { "Alias" }, idxOps.First(c => c.Name == "IX_UserJoin_Alias").Columns);
            Assert.AreEqual(true, idxOps.First(c => c.Name == "IX_UserJoin_Alias").IsUnique);

            CollectionAssert.AreEqual(new[] { "FKConnectorId" }, idxOps.First(c => c.Name == "IX_UserJoin_FKConnectorId").Columns);
            CollectionAssert.AreEqual(new[] { "PKConnectorId" }, idxOps.First(c => c.Name == "IX_UserJoin_PKConnectorId").Columns);

            //Columns
            Assert.AreEqual(8, op.Columns.Count());

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Id").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "Id").IsNullable);
            Assert.AreEqual(128, op.Columns.First(c => c.Name == "Id").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Alias").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "Alias").IsNullable);
            Assert.AreEqual(50, op.Columns.First(c => c.Name == "Alias").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "PKConnectorId").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "PKConnectorId").IsNullable);
            Assert.AreEqual(128, op.Columns.First(c => c.Name == "PKConnectorId").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "FKConnectorId").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "FKConnectorId").IsNullable);
            Assert.AreEqual(128, op.Columns.First(c => c.Name == "FKConnectorId").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "PKTableName").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "PKTableName").IsNullable);
            Assert.AreEqual(100, op.Columns.First(c => c.Name == "PKTableName").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "FKTableName").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "FKTableName").IsNullable);
            Assert.AreEqual(100, op.Columns.First(c => c.Name == "FKTableName").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "PKColumn").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "PKColumn").IsNullable);
            Assert.AreEqual(500, op.Columns.First(c => c.Name == "PKColumn").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "FKColumn").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "FKColumn").IsNullable);
            Assert.AreEqual(500, op.Columns.First(c => c.Name == "FKColumn").MaxLength);
        }

        [TestMethod]
        public void ConnectorSeedData()
        {
            var dataOps = migration.UpOperations.Where(c => c.GetType() == typeof(InsertDataOperation) && ((InsertDataOperation)c).Table == "Connector").Cast<InsertDataOperation>();

            var id1 = dataOps.First(c => (string)c.Values[0, 0] == "4b6b6ba7-0209-4b89-91cb-0e2a67aa37c1");
            CollectionAssert.AreEqual(new[] { "Id", "ConnectionString", "DataSource", "InitialCatalog", "Name", "RecordLimit", "ResourceType", "Schema", "UserId", "IsActive" }, id1.Columns);
            CollectionAssert.AreEqual(new object[] { "4b6b6ba7-0209-4b89-91cb-0e2a67aa37c1", "", null, null, "AppDb", 500, 1, "AutoPoco", null, true }, GetRow(id1.Values, 0));

            var id2 = dataOps.First(c => (string)c.Values[0, 0] == "4d74e770-54e9-4b0f-8f13-59ccb0808654");
            CollectionAssert.AreEqual(new[] { "Id", "ConnectionString", "DataSource", "InitialCatalog", "Name", "RecordLimit", "ResourceType", "Schema", "UserId", "IsActive" }, id2.Columns);
            CollectionAssert.AreEqual(new object[] { "4d74e770-54e9-4b0f-8f13-59ccb0808654", "", null, null, "LogDb", 500, 1, "AutoPocoLog", null, true }, GetRow(id2.Values, 0));
        }


        [TestMethod]
        public void DownOperationsDropsTables()
        {
            var downOps = migration.DownOperations;

            Assert.AreEqual(2, downOps.Count());

            Assert.AreEqual("UserJoin", ((DropTableOperation)downOps[0]).Name);
            Assert.AreEqual("AutoPoco", ((DropTableOperation)downOps[0]).Schema);

            Assert.AreEqual("Connector", ((DropTableOperation)downOps[1]).Name);
            Assert.AreEqual("AutoPoco", ((DropTableOperation)downOps[1]).Schema);
        }

        public T[] GetRow<T>(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

    }
}
