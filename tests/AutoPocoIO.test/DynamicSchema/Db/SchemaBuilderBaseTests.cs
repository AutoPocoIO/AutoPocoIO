using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    
     [Trait("Category", TestCategories.Unit)]
    public partial class SchemaBuilderBaseTests
    {
        private readonly Config config;
        private readonly SchemaBuilder1 builder;
        private readonly DbSchema schema;

        public SchemaBuilderBaseTests()
        {
            config = new Config()
            {
                UsedConnectors = new List<string> { "conn1" }
            };

            schema = new DbSchema();

            builder = new SchemaBuilder1(config, schema, new DbTypeMapper());
            builder.Dts[0] = SchmeaBuilderDataTableBuilder.CreateColumnTable();
        }

        [FactWithName]
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
            Assert.Single(schema.Tables);
            Assert.Single(schema.Columns);
            Assert.Empty(schema.Views);
            Assert.Empty(schema.StoredProcedures);

            //Verify table properties
            var tbl = schema.Tables.First();
            Assert.Equal("tbl1", tbl.Name);
            Assert.Equal("sch1", tbl.Schema);
            Assert.Equal("db1", tbl.Database);
            Assert.Equal("pk123", tbl.PrimaryKeys);

            //Column props
            var col = schema.Columns.First();
            Assert.Equal(col, schema.Columns.First());
            Assert.Equal("sch1", col.TableSchema);
            Assert.Equal("tbl1", col.TableName);
            Assert.Equal("pre123_col1", col.ColumnName);
            Assert.Equal("varchar", col.ColumnType);
            Assert.Equal(1, col.ColumnLength);
            Assert.False(col.ColumnIsNullable);
        }

        [FactWithName]
        public void GetTableSingleColumnsForSchema()
        {
            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "U";
            row["TableName"] = "tbl1";
            builder.Dt.Rows.Add(row);

            builder.GetColumns();

           //Verify object counts/not null
           Assert.Single(schema.Tables);
        }


        [FactWithName]
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
            Assert.Empty(schema.Tables);
            Assert.Equal(2, schema.Columns.Count());
            Assert.Single(schema.Views);
            Assert.Empty(schema.StoredProcedures);

            //Verify table properties
            var tbl = schema.Views.First();
            Assert.Equal("vw1", tbl.Name);
            Assert.Equal("sch1", tbl.Schema);
            Assert.Equal("db1", tbl.Database);

            //Column props
            var col = schema.Columns.First();
            Assert.Equal(col, schema.Columns.First());
            Assert.Equal("sch1", col.TableSchema);
            Assert.Equal("vw1", col.TableName);
            Assert.Equal("col1", col.ColumnName);
            Assert.Equal("vw1", col.View.Name);

            col = schema.Columns.Last();
            Assert.Equal(col, schema.Columns.Last());
            Assert.Equal("sch1", col.TableSchema);
            Assert.Equal("vw1", col.TableName);
            Assert.Equal("col2", col.ColumnName);
            Assert.Equal("vw1", col.View.Name);
        }

        [FactWithName]
        public void DbObjectNotFound()
        {
           
            config.IncludedTable = "tbl123";
            config.UsedConnectors = new List<string> { "conn1" };

            var row = builder.Dt.CreateRowWithColReqValues();
            row["ObjectType"] = "Other";
            builder.Dt.Rows.Add(row);

            void act() => builder.GetColumns();
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [FactWithName]
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
            Assert.Equal("fkName", col.FKName);
            Assert.Equal("db1", col.ReferencedDatabase);
            Assert.Equal("sch2", col.ReferencedSchema);
            Assert.Equal("tbl2", col.ReferencedTable);
            Assert.Equal("colfk2", col.ReferencedColumn);
        }

        [FactWithName]
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
             Assert.Null(col.FKName);
             Assert.Null(col.ReferencedDatabase);
             Assert.Null(col.ReferencedSchema);
             Assert.Null(col.ReferencedTable);
             Assert.Null(col.ReferencedColumn);
        }

        [FactWithName]
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

            Assert.Equal(2, schema.Views.Count());

        }

        [FactWithName]
        public void ExecuteSchemaCommandThrowsExceptionIfCommandNull()
        {
            var builder = new SchmeaBuilder2(new Config(), null, null);
            void act() => builder.ExecuteSchemaCommand(null);
            Assert.Throws<ArgumentNullException>(act);
        }

        [FactWithName]
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


            Assert.Equal(1, results.Rows[0]["Name1"]);
            Assert.Equal(11, results.Rows[1]["Name1"]);
            conn.Verify();

        }
    }
}
