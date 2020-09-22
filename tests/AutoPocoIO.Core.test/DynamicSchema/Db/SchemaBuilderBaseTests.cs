using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public partial class SchemaBuilderBaseTests
    {
        private Config config;
        private SchemaBuilder1 builder;
        private DbSchema schema;

        [TestInitialize]
        public void Init()
        {
            config = new Config()
            {
                UsedConnectors = new List<string> { "conn1" }
            };

            schema = new DbSchema();

            builder = new SchemaBuilder1(config, schema, new DbTypeMapper());
            builder.Dts[0] = SchmeaBuilderDataTableBuilder.CreateColumnTable();
        }

        [TestMethod]
        public void GetTableSingleColumns()
        {
            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1" };
            config.PropertyPreFixName = "pre123_";

            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "U";
            row["TableName"] = "tbl1";
            row["TableSchema"] = "sch1";
            row["DatabaseName"] = "db1";
            row["PKColumnName"] = "pk123";

            row["ColumnName"] = "col1";
            row["ColumnType"] = "varchar";

            builder.Dt.Rows.Add(row);
            builder.GetTableViews();

            //Verify object counts/not null
            Assert.AreEqual(1, schema.Tables.Count());
            Assert.AreEqual(1, schema.Columns.Count());
            Assert.AreEqual(0, schema.Views.Count());
            Assert.AreEqual(0, schema.StoredProcedures.Count());

            //Verify table properties
            var tbl = schema.Tables.First();
            Assert.AreEqual("tbl1", tbl.Name);
            Assert.AreEqual("sch1", tbl.Schema);
            Assert.AreEqual("db1", tbl.Database);
            Assert.AreEqual("pk123", tbl.PrimaryKeys);

            //Column props
            var col = schema.Columns.First();
            Assert.AreSame(col, schema.Columns.First());
            Assert.AreEqual("sch1", col.TableSchema);
            Assert.AreEqual("tbl1", col.TableName);
            Assert.AreEqual("pre123_col1", col.ColumnName);
            Assert.AreEqual("varchar", col.ColumnType);
            Assert.AreEqual(1, col.ColumnLength);
            Assert.AreEqual(false, col.ColumnIsNullable);
        }

        [TestMethod]
        public void EnsureIdentityPKTypesIsNotNull()
        {
            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1" };
            config.PropertyPreFixName = "pre123_";

            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "U";
            row["TableName"] = "tbl1";
            row["TableSchema"] = "sch1";
            row["DatabaseName"] = "db1";
            row["PKColumnName"] = "pk123";
            row["PKIsIdentity"] = "true";

            row["ColumnName"] = "col1";
            row["ColumnType"] = "int";

            builder.Dt.Rows.Add(row);
            builder.GetTableViews();

            var col = schema.Columns.First();
            Assert.AreEqual(typeof(int), col.DataType.SystemType);

        }

        [TestMethod]
        public void GetTableSingleColumnsForSchema()
        {
            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "U";
            row["TableName"] = "tbl1";
            builder.Dt.Rows.Add(row);

            builder.GetColumns();

            //Verify object counts/not null
            Assert.AreEqual(1, schema.Tables.Count());
        }


        [TestMethod]
        public void AddView2Columns()
        {
            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1" };

            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "V";
            row["TableName"] = "vw1";
            row["TableSchema"] = "sch1";
            row["DatabaseName"] = "db1";
            row["ColumnName"] = "col1";

            var row2 = builder.Dt.CreateRowWithColReqValues();
            row2["ObjectType"] = "V";
            row2["TableName"] = "vw1";
            row2["TableSchema"] = "sch1";
            row2["DatabaseName"] = "db1";
            row2["ColumnName"] = "col2";

            builder.Dt.Rows.Add(row);
            builder.Dt.Rows.Add(row2);

            builder.GetTableViews();

            //Verify object counts/not null
            Assert.AreEqual(0, schema.Tables.Count());
            Assert.AreEqual(2, schema.Columns.Count());
            Assert.AreEqual(1, schema.Views.Count());
            Assert.AreEqual(0, schema.StoredProcedures.Count());

            //Verify table properties
            var tbl = schema.Views.First();
            Assert.AreEqual("vw1", tbl.Name);
            Assert.AreEqual("sch1", tbl.Schema);
            Assert.AreEqual("db1", tbl.Database);

            //Column props
            var col = schema.Columns.First();
            Assert.AreSame(col, schema.Columns.First());
            Assert.AreEqual("sch1", col.TableSchema);
            Assert.AreEqual("vw1", col.TableName);
            Assert.AreEqual("col1", col.ColumnName);
            Assert.AreEqual("vw1", col.View.Name);

            col = schema.Columns.Last();
            Assert.AreSame(col, schema.Columns.Last());
            Assert.AreEqual("sch1", col.TableSchema);
            Assert.AreEqual("vw1", col.TableName);
            Assert.AreEqual("col2", col.ColumnName);
            Assert.AreEqual("vw1", col.View.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void DbObjectNotFound()
        {

            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1" };

            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "Other";
            builder.Dt.Rows.Add(row);

            builder.GetColumns();
        }

        [TestMethod]
        public void VerifyFKColumnPropertiesInTable()
        {
            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1" };
            config.PropertyPreFixName = "pre123_";

            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "U";
            row["TableName"] = "tbl1";
            row["TableSchema"] = "sch1";
            row["DatabaseName"] = "db1";
            row["PKColumnName"] = "pk123";

            row["ColumnName"] = "col1";
            row["ColumnType"] = "varchar";

            row["FKName"] = "fkName";
            row["ReferencedSchema"] = "sch2";
            row["ReferencedTable"] = "tbl2";
            row["ReferencedColumn"] = "colfk2";

            builder.Dt.Rows.Add(row);

            builder.GetColumns();

            var col = schema.Columns.First();
            Assert.AreEqual("fkName", col.FKName);
            Assert.AreEqual("db1", col.ReferencedDatabase);
            Assert.AreEqual("sch2", col.ReferencedSchema);
            Assert.AreEqual("tbl2", col.ReferencedTable);
            Assert.AreEqual("colfk2", col.ReferencedColumn);
        }

        [TestMethod]
        public void VerifyFKColumnPropertiesIgnoredInView()
        {
            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1" };
            config.PropertyPreFixName = "pre123_";

            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "V";
            row["TableName"] = "tbl1";
            row["TableSchema"] = "sch1";
            row["DatabaseName"] = "db1";
            row["PKColumnName"] = "pk123";

            row["ColumnName"] = "col1";
            row["ColumnType"] = "varchar";

            row["FKName"] = "fkName";
            row["ReferencedSchema"] = "sch2";
            row["ReferencedTable"] = "tbl2";
            row["ReferencedColumn"] = "colfk2";

            builder.Dt.Rows.Add(row);

            builder.GetColumns();

            var col = schema.Columns.First();
            Assert.IsNull(col.FKName);
            Assert.IsNull(col.ReferencedDatabase);
            Assert.IsNull(col.ReferencedSchema);
            Assert.IsNull(col.ReferencedTable);
            Assert.IsNull(col.ReferencedColumn);
        }

        [TestMethod]
        public void MergeMultipleConnectors()
        {


            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1", "conn2" };
            var dt1 = builder.Dt.Clone();
            var dt2 = builder.Dt.Clone();

            var row1 = dt1.CreateRowWithColReqValues();
            row1["ObjectType"] = "V";
            row1["TableName"] = "tbl1";
            dt1.Rows.Add(row1);

            var row2 = dt2.CreateRowWithColReqValues();
            row2["ObjectType"] = "V";
            row2["TableName"] = "tbl2";
            dt2.Rows.Add(row2);

            builder.Dts = new[] { dt1, dt2 };
            builder.GetColumns();

            Assert.AreEqual(2, schema.Views.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteSchemaCommandThrowsExceptionIfCommandNull()
        {
            var builder = new SchmeaBuilder2(new Config(), null, null);
            builder.ExecuteSchemaCommand(null);
        }

        [TestMethod]
        public void ExcueteSchemaCommandReturnsSchemaDataTable()
        {
            int readerRow = 0;
            var readerData = new[] { 1, 11 };

            var conn = new Mock<IDbConnection>();
            conn.Setup(c => c.Open()).Verifiable();

            var reader = new Mock<IDataReader>();
            reader.Setup(c => c.FieldCount).Returns(1);
            reader.Setup(c => c.GetName(0)).Returns("Name1");
            reader.Setup(c => c.GetFieldType(0)).Returns(typeof(int));

            reader.Setup(r => r.GetValues(It.IsAny<object[]>()))
                .Callback<object[]>(c => c[0] = readerData[readerRow++])
                .Returns(1);

            reader.SetupSequence(m => m.Read())
                      .Returns(true) // Read the first row
                      .Returns(true) // Read the second row
                      .Returns(false); // Done reading

            var command = new Mock<IDbCommand>();
            command.Setup(c => c.Connection).Returns(conn.Object);
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);


            var results = builder.ExecuteSchemaCommandBase(command.Object);


            Assert.AreEqual(1, results.Rows[0]["Name1"]);
            Assert.AreEqual(11, results.Rows[1]["Name1"]);
            conn.Verify();

        }
    }
}
