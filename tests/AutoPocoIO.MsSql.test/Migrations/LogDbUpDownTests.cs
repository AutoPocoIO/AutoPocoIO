using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AutoPocoIO.test.Migrations
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class LogDbUpDownTests
    {
        private class LogDBMigration : LogDb
        {
            public void UpPublic(MigrationBuilder modelBuilder) => base.Up(modelBuilder);
            public void DownPublic(MigrationBuilder modelBuilder) => base.Down(modelBuilder);

        }

        private LogDBMigration migration;

        [TestInitialize]
        public void Init()
        {
            var builder = new MigrationBuilder("System.Data.SqlClient");

            migration = new LogDBMigration();
            migration.UpPublic(builder);
            migration.DownPublic(builder);
        }

        [TestMethod]
        public void EnsureAutoPocoLogSchema()
        {
            Assert.AreEqual("AutoPocoLog", ((EnsureSchemaOperation)migration.UpOperations[0]).Name);
        }

        [TestMethod]
        public void RequestLogTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "Request");

            Assert.AreEqual("AutoPocoLog", op.Schema);

            //PK
            Assert.AreEqual("PK_Request", op.PrimaryKey.Name);
            CollectionAssert.AreEqual(new[] { "RequestId", "RequestGuid" }, op.PrimaryKey.Columns);

            //FK
            Assert.AreEqual(0, op.ForeignKeys.Count());

            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "Request").Cast<CreateIndexOperation>();

            Assert.AreEqual(4, idxOps.Count());
            CollectionAssert.AreEqual(new[] { "DateTimeUtc" }, idxOps.First(c => c.Name == "IX_Request_DateTimeUtc").Columns);
            CollectionAssert.AreEqual(new[] { "DayOfRequest" }, idxOps.First(c => c.Name == "IX_DayWithIP").Columns);
            CollectionAssert.AreEqual(new[] { "RequesterIp" }, (string[])idxOps.First(c => c.Name == "IX_DayWithIP").FindAnnotation("SqlServer:Include").Value);
            CollectionAssert.AreEqual(new[] { "RequestGuid" }, idxOps.First(c => c.Name == "IX_Request_RequestGuid").Columns);
            CollectionAssert.AreEqual(new[] { "DateTimeUtc", "RequestType", "RequesterIp", "Connector" }, (string[])idxOps.First(c => c.Name == "IX_Request_RequestGuid").FindAnnotation("SqlServer:Include").Value);
            CollectionAssert.AreEqual(new[] { "DayOfRequest", "RequestType" }, idxOps.First(c => c.Name == "IX_DayAndType").Columns);


            //Columns
            Assert.AreEqual(7, op.Columns.Count());

            Assert.AreEqual(typeof(long), op.Columns.First(c => c.Name == "RequestId").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "RequestId").IsNullable);

            Assert.AreEqual(typeof(Guid), op.Columns.First(c => c.Name == "RequestGuid").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "RequestGuid").IsNullable);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "RequesterIp").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "RequesterIp").IsNullable);
            Assert.AreEqual(39, op.Columns.First(c => c.Name == "RequesterIp").MaxLength);

            Assert.AreEqual(typeof(DateTime), op.Columns.First(c => c.Name == "DateTimeUtc").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "DateTimeUtc").IsNullable);
            Assert.AreEqual("datetime2(4)", op.Columns.First(c => c.Name == "DateTimeUtc").ColumnType);

            Assert.AreEqual(typeof(DateTime), op.Columns.First(c => c.Name == "DayOfRequest").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "DayOfRequest").IsNullable);
            Assert.AreEqual("CONVERT(date, DateTimeUtc)", op.Columns.First(c => c.Name == "DayOfRequest").ComputedColumnSql);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "RequestType").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "RequestType").IsNullable);
            Assert.AreEqual(10, op.Columns.First(c => c.Name == "RequestType").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Connector").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "Connector").IsNullable);
            Assert.AreEqual(50, op.Columns.First(c => c.Name == "Connector").MaxLength);

        }

        [TestMethod]
        public void ResponseLogTableOp()
        {
            CreateTableOperation op = (CreateTableOperation)migration.UpOperations.First(c => c.GetType() == typeof(CreateTableOperation) && ((CreateTableOperation)c).Name == "Response");

            Assert.AreEqual("AutoPocoLog", op.Schema);

            //PK
            Assert.AreEqual("PK_Response", op.PrimaryKey.Name);
            CollectionAssert.AreEqual(new[] { "ResponseId", "RequestGuid" }, op.PrimaryKey.Columns);

            //FK
            Assert.AreEqual(0, op.ForeignKeys.Count());

            //Index
            var idxOps = migration.UpOperations.Where(c => c.GetType() == typeof(CreateIndexOperation) && ((CreateIndexOperation)c).Table == "Response").Cast<CreateIndexOperation>();
            Assert.AreEqual(0, idxOps.Count());


            //Columns
            Assert.AreEqual(6, op.Columns.Count());

            Assert.AreEqual(typeof(long), op.Columns.First(c => c.Name == "ResponseId").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "ResponseId").IsNullable);

            Assert.AreEqual(typeof(Guid), op.Columns.First(c => c.Name == "RequestGuid").ClrType);
            Assert.IsFalse(op.Columns.First(c => c.Name == "RequestGuid").IsNullable);

            Assert.AreEqual(typeof(DateTime), op.Columns.First(c => c.Name == "DateTimeUtc").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "DateTimeUtc").IsNullable);
            Assert.AreEqual("datetime2(4)", op.Columns.First(c => c.Name == "DateTimeUtc").ColumnType);

            Assert.AreEqual(typeof(DateTime), op.Columns.First(c => c.Name == "DayOfResponse").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "DayOfResponse").IsNullable);
            Assert.AreEqual("CONVERT(date, DateTimeUtc)", op.Columns.First(c => c.Name == "DayOfResponse").ComputedColumnSql);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Status").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "Status").IsNullable);
            Assert.AreEqual(51, op.Columns.First(c => c.Name == "Status").MaxLength);

            Assert.AreEqual(typeof(string), op.Columns.First(c => c.Name == "Exception").ClrType);
            Assert.IsTrue(op.Columns.First(c => c.Name == "Exception").IsNullable);
        }

        [TestMethod]
        public void DownOperationsDropsTables()
        {
            var downOps = migration.DownOperations;

            Assert.AreEqual(2, downOps.Count());

            Assert.AreEqual("Request", ((DropTableOperation)downOps[0]).Name);
            Assert.AreEqual("AutoPocoLog", ((DropTableOperation)downOps[0]).Schema);

            Assert.AreEqual("Response", ((DropTableOperation)downOps[1]).Name);
            Assert.AreEqual("AutoPocoLog", ((DropTableOperation)downOps[1]).Schema);
        }
    }
}
