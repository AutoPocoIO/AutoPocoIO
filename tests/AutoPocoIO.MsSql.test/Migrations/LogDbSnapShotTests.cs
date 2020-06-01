using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AutoPocoIO.test.Migrations
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class LogDbSnapShotTests
    {
        private class LogDBSnapShot : LogDbContextModelSnapshot
        {
            public void BuildModelPublic(ModelBuilder modelBuilder) => base.BuildModel(modelBuilder);
        }

        private LogDBSnapShot snapShot;

        [TestInitialize]
        public void Init()
        {
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);

            snapShot = new LogDBSnapShot();
            snapShot.BuildModelPublic(builder);
        }

        [TestMethod]
        public void BuilderHasAnnotations()
        {
            Assert.AreEqual("2.2.4-servicing-10062", snapShot.Model.FindAnnotation("ProductVersion").Value);
            Assert.AreEqual(128, snapShot.Model.FindAnnotation("Relational:MaxIdentifierLength").Value);
            Assert.AreEqual(SqlServerValueGenerationStrategy.IdentityColumn, snapShot.Model.FindAnnotation("SqlServer:ValueGenerationStrategy").Value);
        }

        [TestMethod]
        public void RequestLogTableSetup()
        {
            var entity = snapShot.Model.FindEntityType("AutoPoco.Logging.Models.RequestLog");

            //Table Metadata
            Assert.AreEqual("AutoPocoLog", entity.Relational().Schema);
            Assert.AreEqual("Request", entity.Relational().TableName);

            //PK
            Assert.AreEqual(2, entity.FindPrimaryKey().Properties.Count());
            Assert.AreEqual(entity.FindProperty("RequestId"), entity.FindPrimaryKey().Properties.First());
            Assert.AreEqual(entity.FindProperty("RequestGuid"), entity.FindPrimaryKey().Properties.Last());

            //FK
            var fks = entity.GetNavigations();
            Assert.AreEqual(0, fks.Count());

            //Index
            Assert.IsFalse(entity.GetIndexes().First(c => c.Relational().Name == "IX_Request_DateTimeUtc").IsUnique);
            Assert.IsFalse(entity.GetIndexes().First(c => c.Relational().Name == "IX_DayWithIP").IsUnique);
            CollectionAssert.AreEqual(new[] { "RequesterIp" }, (string[])entity.GetIndexes().First(c => c.Relational().Name == "IX_DayWithIP").FindAnnotation("SqlServer:Include").Value);

            Assert.IsFalse(entity.GetIndexes().First(c => c.Relational().Name == "IX_Request_RequestGuid").IsUnique);
            CollectionAssert.AreEqual(new[] { "DateTimeUtc", "RequestType", "RequesterIp", "Connector" }, (string[])entity.GetIndexes().First(c => c.Relational().Name == "IX_Request_RequestGuid").FindAnnotation("SqlServer:Include").Value);

            Assert.IsFalse(entity.GetIndexes().First(c => c.Relational().Name == "IX_DayAndType").IsUnique);
            Assert.AreEqual(entity.FindProperty("DayOfRequest"), entity.GetIndexes().First(c => c.Relational().Name == "IX_DayAndType").Properties.First());
            Assert.AreEqual(entity.FindProperty("RequestType"), entity.GetIndexes().First(c => c.Relational().Name == "IX_DayAndType").Properties.Last());

            //Columns (if column not found then will throw nullobject)
            Assert.AreEqual(7, entity.GetProperties().Count());

            Assert.AreEqual(typeof(long), entity.FindProperty("RequestId").ClrType);
            Assert.AreEqual(typeof(Guid), entity.FindProperty("RequestGuid").ClrType);
            Assert.AreEqual(typeof(string), entity.FindProperty("Connector").ClrType);
            Assert.AreEqual(50, entity.FindProperty("Connector").GetMaxLength());

            Assert.AreEqual(typeof(DateTime?), entity.FindProperty("DateTimeUtc").ClrType);
            Assert.AreEqual("datetime2(4)", entity.FindProperty("DateTimeUtc").Relational().ColumnType);

            Assert.AreEqual(typeof(DateTime?), entity.FindProperty("DayOfRequest").ClrType);
            Assert.AreEqual(ValueGenerated.OnAddOrUpdate, entity.FindProperty("DayOfRequest").ValueGenerated);
            Assert.AreEqual("CONVERT(date, DateTimeUtc)", entity.FindProperty("DayOfRequest").SqlServer().ComputedColumnSql);

            Assert.AreEqual(typeof(string), entity.FindProperty("RequestType").ClrType);
            Assert.AreEqual(10, entity.FindProperty("RequestType").GetMaxLength());

            Assert.AreEqual(typeof(string), entity.FindProperty("RequesterIp").ClrType);
            Assert.AreEqual(39, entity.FindProperty("RequesterIp").GetMaxLength());

        }

        [TestMethod]
        public void ResponseLogTableSetup()
        {
            var entity = snapShot.Model.FindEntityType("AutoPoco.Logging.Models.ResponseLog");

            //Table Metadata
            Assert.AreEqual("AutoPocoLog", entity.Relational().Schema);
            Assert.AreEqual("Response", entity.Relational().TableName);

            //PK
            Assert.AreEqual(2, entity.FindPrimaryKey().Properties.Count());
            Assert.AreEqual(entity.FindProperty("ResponseId"), entity.FindPrimaryKey().Properties.First());
            Assert.AreEqual(entity.FindProperty("RequestGuid"), entity.FindPrimaryKey().Properties.Last());

            //FK
            var fks = entity.GetNavigations();
            Assert.AreEqual(0, fks.Count());

            //Columns (if column not found then will throw nullobject)
            Assert.AreEqual(6, entity.GetProperties().Count());

            Assert.AreEqual(typeof(long), entity.FindProperty("ResponseId").ClrType);
            Assert.AreEqual(typeof(Guid), entity.FindProperty("RequestGuid").ClrType);
            Assert.AreEqual(typeof(DateTime?), entity.FindProperty("DateTimeUtc").ClrType);
            Assert.AreEqual("datetime2(4)", entity.FindProperty("DateTimeUtc").Relational().ColumnType);

            Assert.AreEqual(typeof(DateTime?), entity.FindProperty("DayOfResponse").ClrType);
            Assert.AreEqual(ValueGenerated.OnAddOrUpdate, entity.FindProperty("DayOfResponse").ValueGenerated);
            Assert.AreEqual("CONVERT(date, DateTimeUtc)", entity.FindProperty("DayOfResponse").SqlServer().ComputedColumnSql);

            Assert.AreEqual(typeof(string), entity.FindProperty("Status").ClrType);
            Assert.AreEqual(51, entity.FindProperty("Status").GetMaxLength());
        }
    }
}
