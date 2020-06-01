using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;
using System.Linq;

namespace AutoPocoIO.MsSql.test.Migrations
{
    
    [Trait("Category", TestCategories.Unit)]
    public class AppDbUpDownTests
    {
        private class AppDBMigration : AppDb
        {
            public void UpPublic(MigrationBuilder modelBuilder) => base.Up(modelBuilder);
            public void DownPublic(MigrationBuilder modelBuilder) => base.Down(modelBuilder);

        }

        private readonly AppDBMigration migration;
        public AppDbUpDownTests()
        {
            var builder = new MigrationBuilder("System.Data.SqlClient");

            migration = new AppDBMigration();
            migration.UpPublic(builder);
            migration.DownPublic(builder);
        }

        [FactWithName]
        public void EnsureAutoPocoSchema()
        {
            Assert.Equal("AutoPoco", ((EnsureSchemaOperation)migration.UpOperations[0]).Name);
        }

        [FactWithName]
        public void ConnectorTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "Connector");

            Assert.Equal("AutoPoco", op.Schema);

            //PK
            Assert.Equal("PK_Connector", op.PrimaryKey.Name);
            Assert.Equal(new[] { "Id" }, op.PrimaryKey.Columns);

            //FK
            Assert.Empty(op.ForeignKeys);

            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "Connector").Cast<CreateIndexOperation>();
            Assert.Single(idxOps);

            Assert.Equal(new[] { "Name" }, idxOps.First(c => c.Name == "IDX_ConnectorName").Columns);
            Assert.True(idxOps.First(c => c.Name == "IDX_ConnectorName").IsUnique);
            Assert.Equal("[Name] IS NOT NULL", idxOps.First(c => c.Name == "IDX_ConnectorName").Filter);

            //Columns
            Assert.Equal(11, op.Columns.Count());

            Assert.Equal(typeof(int), op.Columns.First(c => c.Name == "Id").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "Id").IsNullable);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, op.Columns.First(c => c.Name == "Id").FindAnnotation("SqlServer:ValueGenerationStrategy").Value);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "Name").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "Name").IsNullable);
            Assert.Equal(50, op.Columns.First(c => c.Name == "Name").MaxLength);

            Assert.Equal(typeof(int), op.Columns.First(c => c.Name == "ResourceType").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "ResourceType").IsNullable);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "Schema").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "Schema").IsNullable);
            Assert.Equal(50, op.Columns.First(c => c.Name == "Schema").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "ConnectionString").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "ConnectionString").IsNullable);

            Assert.Equal(typeof(int), op.Columns.First(c => c.Name == "RecordLimit").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "RecordLimit").IsNullable);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "UserId").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "UserId").IsNullable);
            Assert.Equal(50, op.Columns.First(c => c.Name == "UserId").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "InitialCatalog").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "InitialCatalog").IsNullable);
            Assert.Equal(50, op.Columns.First(c => c.Name == "InitialCatalog").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "DataSource").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "DataSource").IsNullable);
            Assert.Equal(500, op.Columns.First(c => c.Name == "DataSource").MaxLength);

            Assert.Equal(typeof(int), op.Columns.First(c => c.Name == "Port").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "Port").IsNullable);

            Assert.Equal(typeof(bool), op.Columns.First(c => c.Name == "IsActive").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "IsActive").IsNullable);

        }

        [FactWithName]
        public void UserJoinTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "UserJoin");

            Assert.Equal("AutoPoco", op.Schema);

            //PK
            Assert.Equal("PK_UserJoin", op.PrimaryKey.Name);
            Assert.Equal(new[] { "Id" }, op.PrimaryKey.Columns);

            //FK
            var fk = op.ForeignKeys.First(c => c.Name == "FK_UserJoin_Connector_FKConnectorId");
            Assert.Equal(new[] { "FKConnectorId" }, fk.Columns);
            Assert.Equal("AutoPoco.Connector.Id", $"{fk.PrincipalSchema}.{fk.PrincipalTable}.{fk.PrincipalColumns[0]}");
            Assert.Equal(ReferentialAction.Restrict, fk.OnDelete);

            fk = op.ForeignKeys.First(c => c.Name == "FK_UserJoin_Connector_PKConnectorId");
            Assert.Equal(new[] { "PKConnectorId" }, fk.Columns);
            Assert.Equal("AutoPoco.Connector.Id", $"{fk.PrincipalSchema}.{fk.PrincipalTable}.{fk.PrincipalColumns[0]}");
            Assert.Equal(ReferentialAction.Restrict, fk.OnDelete);


            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "UserJoin").Cast<CreateIndexOperation>();
            Assert.Equal(3, idxOps.Count());

            Assert.Equal(new[] { "Alias" }, idxOps.First(c => c.Name == "IX_UserJoin_Alias").Columns);
            Assert.True(idxOps.First(c => c.Name == "IX_UserJoin_Alias").IsUnique);

            Assert.Equal(new[] { "FKConnectorId" }, idxOps.First(c => c.Name == "IX_UserJoin_FKConnectorId").Columns);
            Assert.Equal(new[] { "PKConnectorId" }, idxOps.First(c => c.Name == "IX_UserJoin_PKConnectorId").Columns);

            //Columns
            Assert.Equal(8, op.Columns.Count());

            Assert.Equal(typeof(int), op.Columns.First(c => c.Name == "Id").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "Id").IsNullable);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, op.Columns.First(c => c.Name == "Id").FindAnnotation("SqlServer:ValueGenerationStrategy").Value);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "Alias").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "Alias").IsNullable);
            Assert.Equal(50, op.Columns.First(c => c.Name == "Alias").MaxLength);

            Assert.Equal(typeof(int), op.Columns.First(c => c.Name == "PKConnectorId").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "PKConnectorId").IsNullable);

            Assert.Equal(typeof(int), op.Columns.First(c => c.Name == "FKConnectorId").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "FKConnectorId").IsNullable);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "PKTableName").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "PKTableName").IsNullable);
            Assert.Equal(100, op.Columns.First(c => c.Name == "PKTableName").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "FKTableName").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "FKTableName").IsNullable);
            Assert.Equal(100, op.Columns.First(c => c.Name == "FKTableName").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "PKColumn").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "PKColumn").IsNullable);
            Assert.Equal(500, op.Columns.First(c => c.Name == "PKColumn").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "FKColumn").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "FKColumn").IsNullable);
            Assert.Equal(500, op.Columns.First(c => c.Name == "FKColumn").MaxLength);
        }

        [FactWithName]
        public void ConnectorSeedData()
        {
            var dataOps = migration.UpOperations.Where(c => c.GetType() == typeof(InsertDataOperation) && ((InsertDataOperation)c).Table == "Connector").Cast<InsertDataOperation>();

            var id1 = dataOps.First(c => (int)c.Values[0, 0] == 1);
            Assert.Equal(new[] { "Id", "ConnectionString", "DataSource", "InitialCatalog", "Name", "RecordLimit", "ResourceType", "Schema", "UserId", "IsActive" }, id1.Columns);
            Assert.Equal(new object[] { 1, "", null, null, "appDb", 500, 1, "AutoPoco", null, true }, GetRow(id1.Values, 0));

            var id2 = dataOps.First(c => (int)c.Values[0, 0] == 2);
            Assert.Equal(new[] { "Id", "ConnectionString", "DataSource", "InitialCatalog", "Name", "RecordLimit", "ResourceType", "Schema", "UserId", "IsActive" }, id2.Columns);
            Assert.Equal(new object[] { 2, "", null, null, "logDb", 500, 1, "AutoPocoLog", null, true }, GetRow(id2.Values, 0));
        }


        [FactWithName]
        public void DownOperationsDropsTables()
        {
            var downOps = migration.DownOperations;

            Assert.Equal(2, downOps.Count());

            Assert.Equal("UserJoin", ((DropTableOperation)downOps[0]).Name);
            Assert.Equal("AutoPoco", ((DropTableOperation)downOps[0]).Schema);

            Assert.Equal("Connector", ((DropTableOperation)downOps[1]).Name);
            Assert.Equal("AutoPoco", ((DropTableOperation)downOps[1]).Schema);
        }

        public T[] GetRow<T>(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

    }
}
