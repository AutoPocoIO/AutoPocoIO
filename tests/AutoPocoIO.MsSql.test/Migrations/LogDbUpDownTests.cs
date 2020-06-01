using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;
using System;
using System.Linq;

namespace AutoPocoIO.MsSql.test.Migrations
{
    
    [Trait("Category", TestCategories.Unit)]
    public class LogDbUpDownTests
    {
        private class LogDBMigration : LogDb
        {
            public void UpPublic(MigrationBuilder modelBuilder) => base.Up(modelBuilder);
            public void DownPublic(MigrationBuilder modelBuilder) => base.Down(modelBuilder);

        }

        private LogDBMigration migration;
        public LogDbUpDownTests()
        {
            var builder = new MigrationBuilder("System.Data.SqlClient");

            migration = new LogDBMigration();
            migration.UpPublic(builder);
            migration.DownPublic(builder);
        }

        [FactWithName]
        public void EnsureAutoPocoLogSchema()
        {
            Assert.Equal("AutoPocoLog", ((EnsureSchemaOperation)migration.UpOperations[0]).Name);
        }

        [FactWithName]
        public void RequestLogTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "Request");

            Assert.Equal("AutoPocoLog", op.Schema);

            //PK
            Assert.Equal("PK_Request", op.PrimaryKey.Name);
            Assert.Equal(new[] { "RequestId", "RequestGuid" }, op.PrimaryKey.Columns);

            //FK
            Assert.Empty(op.ForeignKeys);

            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "Request").Cast<CreateIndexOperation>();

            Assert.Equal(4, idxOps.Count());
            Assert.Equal(new[] { "DateTimeUtc" }, idxOps.First(c => c.Name == "IX_Request_DateTimeUtc").Columns);
            Assert.Equal(new[] { "DayOfRequest" }, idxOps.First(c => c.Name == "IX_DayWithIP").Columns);
            Assert.Equal(new[] { "RequesterIp" }, (string[])idxOps.First(c => c.Name == "IX_DayWithIP").FindAnnotation("SqlServer:Include").Value);
            Assert.Equal(new[] { "RequestGuid" }, idxOps.First(c => c.Name == "IX_Request_RequestGuid").Columns);
            Assert.Equal(new[] { "DateTimeUtc", "RequestType", "RequesterIp", "Connector" }, (string[])idxOps.First(c => c.Name == "IX_Request_RequestGuid").FindAnnotation("SqlServer:Include").Value);
            Assert.Equal(new[] { "DayOfRequest", "RequestType" }, idxOps.First(c => c.Name == "IX_DayAndType").Columns);


            //Columns
            Assert.Equal(7, op.Columns.Count());

            Assert.Equal(typeof(long), op.Columns.First(c => c.Name == "RequestId").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "RequestId").IsNullable);

            Assert.Equal(typeof(Guid), op.Columns.First(c => c.Name == "RequestGuid").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "RequestGuid").IsNullable);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "RequesterIp").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "RequesterIp").IsNullable);
            Assert.Equal(39, op.Columns.First(c => c.Name == "RequesterIp").MaxLength);

            Assert.Equal(typeof(DateTime), op.Columns.First(c => c.Name == "DateTimeUtc").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "DateTimeUtc").IsNullable);
            Assert.Equal("datetime2(4)", op.Columns.First(c => c.Name == "DateTimeUtc").ColumnType);

            Assert.Equal(typeof(DateTime), op.Columns.First(c => c.Name == "DayOfRequest").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "DayOfRequest").IsNullable);
            Assert.Equal("CONVERT(date, DateTimeUtc)", op.Columns.First(c => c.Name == "DayOfRequest").ComputedColumnSql);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "RequestType").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "RequestType").IsNullable);
            Assert.Equal(10, op.Columns.First(c => c.Name == "RequestType").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "Connector").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "Connector").IsNullable);
            Assert.Equal(50, op.Columns.First(c => c.Name == "Connector").MaxLength);

        }

        [FactWithName]
        public void ResponseLogTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "Response");

            Assert.Equal("AutoPocoLog", op.Schema);

            //PK
            Assert.Equal("PK_Response", op.PrimaryKey.Name);
            Assert.Equal(new[] { "ResponseId", "RequestGuid" }, op.PrimaryKey.Columns);

            //FK
            Assert.Empty(op.ForeignKeys);

            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "Response").Cast<CreateIndexOperation>();
            Assert.Empty(idxOps);


            //Columns
            Assert.Equal(6, op.Columns.Count());

            Assert.Equal(typeof(long), op.Columns.First(c => c.Name == "ResponseId").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "ResponseId").IsNullable);

            Assert.Equal(typeof(Guid), op.Columns.First(c => c.Name == "RequestGuid").ClrType);
            Assert.False(op.Columns.First(c => c.Name == "RequestGuid").IsNullable);

            Assert.Equal(typeof(DateTime), op.Columns.First(c => c.Name == "DateTimeUtc").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "DateTimeUtc").IsNullable);
            Assert.Equal("datetime2(4)", op.Columns.First(c => c.Name == "DateTimeUtc").ColumnType);

            Assert.Equal(typeof(DateTime), op.Columns.First(c => c.Name == "DayOfResponse").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "DayOfResponse").IsNullable);
            Assert.Equal("CONVERT(date, DateTimeUtc)", op.Columns.First(c => c.Name == "DayOfResponse").ComputedColumnSql);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "Status").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "Status").IsNullable);
            Assert.Equal(51, op.Columns.First(c => c.Name == "Status").MaxLength);

            Assert.Equal(typeof(string), op.Columns.First(c => c.Name == "Exception").ClrType);
            Assert.True(op.Columns.First(c => c.Name == "Exception").IsNullable);
        }

        [FactWithName]
        public void DownOperationsDropsTables()
        {
            var downOps = migration.DownOperations;

            Assert.Equal(2, downOps.Count());

            Assert.Equal("Request", ((DropTableOperation)downOps[0]).Name);
            Assert.Equal("AutoPocoLog", ((DropTableOperation)downOps[0]).Schema);

            Assert.Equal("Response", ((DropTableOperation)downOps[1]).Name);
            Assert.Equal("AutoPocoLog", ((DropTableOperation)downOps[1]).Schema);
        }
    }
}
