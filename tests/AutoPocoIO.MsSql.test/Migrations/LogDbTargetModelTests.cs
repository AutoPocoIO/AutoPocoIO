using AutoPocoIO.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;
using System;
using System.Linq;

namespace AutoPocoIO.MsSql.test.Migrations
{
    
    [Trait("Category", TestCategories.Unit)]
    public class LogDbTargetModelTests
    {
        private class LogDBSnapShot : LogDb
        {
            public void BuildTargetModelPublic(ModelBuilder modelBuilder) => base.BuildTargetModel(modelBuilder);

            public IModel Model { get; set; }
        }

        private LogDBSnapShot migration;

        public LogDbTargetModelTests()
        {
            var conventionSet = new ConventionSet();
            var builder = new ModelBuilder(conventionSet);

            migration = new LogDBSnapShot();
            migration.BuildTargetModelPublic(builder);

            migration.Model = builder.FinalizeModel();
        }

        [FactWithName]
        public void BuilderHasAnnotations()
        {
            Assert.Equal("2.2.4-servicing-10062", migration.Model.FindAnnotation("ProductVersion").Value);
            Assert.Equal(128, migration.Model.FindAnnotation("Relational:MaxIdentifierLength").Value);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, migration.Model.FindAnnotation("SqlServer:ValueGenerationStrategy").Value);
        }

        [FactWithName]
        public void RequestLogTableSetup()
        {
            var entity = migration.Model.FindEntityType("AutoPoco.Logging.Models.RequestLog");

            //Table Metadata
            Assert.Equal("AutoPocoLog", entity.Relational().Schema);
            Assert.Equal("Request", entity.Relational().TableName);

            //PK
            Assert.Equal(2, entity.FindPrimaryKey().Properties.Count());
            Assert.Equal(entity.FindProperty("RequestId"), entity.FindPrimaryKey().Properties.First());
            Assert.Equal(entity.FindProperty("RequestGuid"), entity.FindPrimaryKey().Properties.Last());

            //FK
            var fks = entity.GetNavigations();
            Assert.Empty(fks);

            //Index
            Assert.False(entity.GetIndexes().First(c => c.Relational().Name == "IX_Request_DateTimeUtc").IsUnique);
            Assert.False(entity.GetIndexes().First(c => c.Relational().Name == "IX_DayWithIP").IsUnique);
            Assert.Equal(new[] { "RequesterIp" }, (string[])entity.GetIndexes().First(c => c.Relational().Name == "IX_DayWithIP").FindAnnotation("SqlServer:Include").Value);

            Assert.False(entity.GetIndexes().First(c => c.Relational().Name == "IX_Request_RequestGuid").IsUnique);
            Assert.Equal(new[] { "DateTimeUtc", "RequestType", "RequesterIp", "Connector" }, (string[])entity.GetIndexes().First(c => c.Relational().Name == "IX_Request_RequestGuid").FindAnnotation("SqlServer:Include").Value);

            Assert.False(entity.GetIndexes().First(c => c.Relational().Name == "IX_DayAndType").IsUnique);
            Assert.Equal(entity.FindProperty("DayOfRequest"), entity.GetIndexes().First(c => c.Relational().Name == "IX_DayAndType").Properties.First());
            Assert.Equal(entity.FindProperty("RequestType"), entity.GetIndexes().First(c => c.Relational().Name == "IX_DayAndType").Properties.Last());



            //Columns (if column not found then will throw nullobject)
            Assert.Equal(7, entity.GetProperties().Count());

            Assert.Equal(typeof(long), entity.FindProperty("RequestId").ClrType);
            Assert.Equal(typeof(Guid), entity.FindProperty("RequestGuid").ClrType);
            Assert.Equal(typeof(string), entity.FindProperty("Connector").ClrType);
            Assert.Equal(50, entity.FindProperty("Connector").GetMaxLength());

            Assert.Equal(typeof(DateTime?), entity.FindProperty("DateTimeUtc").ClrType);
            Assert.Equal("datetime2(4)", entity.FindProperty("DateTimeUtc").Relational().ColumnType);

            Assert.Equal(typeof(DateTime?), entity.FindProperty("DayOfRequest").ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entity.FindProperty("DayOfRequest").ValueGenerated);
            Assert.Equal("CONVERT(date, DateTimeUtc)", entity.FindProperty("DayOfRequest").SqlServer().ComputedColumnSql);

            Assert.Equal(typeof(string), entity.FindProperty("RequestType").ClrType);
            Assert.Equal(10, entity.FindProperty("RequestType").GetMaxLength());

            Assert.Equal(typeof(string), entity.FindProperty("RequesterIp").ClrType);
            Assert.Equal(39, entity.FindProperty("RequesterIp").GetMaxLength());

        }
        [FactWithName]
        public void ResponseLogTableSetup()
        {
            var entity = migration.Model.FindEntityType("AutoPoco.Logging.Models.ResponseLog");

            //Table Metadata
            Assert.Equal("AutoPocoLog", entity.Relational().Schema);
            Assert.Equal("Response", entity.Relational().TableName);

            //PK
            Assert.Equal(2, entity.FindPrimaryKey().Properties.Count());
            Assert.Equal(entity.FindProperty("ResponseId"), entity.FindPrimaryKey().Properties.First());
            Assert.Equal(entity.FindProperty("RequestGuid"), entity.FindPrimaryKey().Properties.Last());

            //FK
            var fks = entity.GetNavigations();
            Assert.Empty(fks);

            //Columns (if column not found then will throw nullobject)
            Assert.Equal(6, entity.GetProperties().Count());

            Assert.Equal(typeof(long), entity.FindProperty("ResponseId").ClrType);
            Assert.Equal(typeof(Guid), entity.FindProperty("RequestGuid").ClrType);
            Assert.Equal(typeof(DateTime?), entity.FindProperty("DateTimeUtc").ClrType);
            Assert.Equal("datetime2(4)", entity.FindProperty("DateTimeUtc").Relational().ColumnType);

            Assert.Equal(typeof(DateTime?), entity.FindProperty("DayOfResponse").ClrType);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entity.FindProperty("DayOfResponse").ValueGenerated);
            Assert.Equal("CONVERT(date, DateTimeUtc)", entity.FindProperty("DayOfResponse").SqlServer().ComputedColumnSql);

            Assert.Equal(typeof(string), entity.FindProperty("Status").ClrType);
            Assert.Equal(51, entity.FindProperty("Status").GetMaxLength());
        }
    }
}
