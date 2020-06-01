using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using Xunit;
using System.Collections.Generic;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    
     [Trait("Category", TestCategories.Unit)]
    public class TableHashCodeTests
    {
        [FactWithName]
        public void ColumnHashIsNotMemoryBased()
        {
            Column col1 = new Column()
            {
                ColumnName = "col1",
                TableName = "tbl1",
                TableSchema = "sch1",
                ColumnType = "type"
            };

            Column col2 = new Column()
            {
                ColumnName = "col1",
                TableName = "tbl1",
                TableSchema = "sch1",
                ColumnType = "type"
            };

            Assert.Equal(col1.GetHashCode(), col2.GetHashCode());
        }

        [FactWithName]
        public void ColumnHashIsFKDependent()
        {
            Column col1 = new Column()
            {
                ColumnName = "col1",
                TableName = "tbl1",
                TableSchema = "sch1",
                ColumnType = "type"
            };

            Column col2 = new Column()
            {
                ColumnName = "col1",
                TableName = "tbl1",
                TableSchema = "sch1",
                ColumnType = "type",
                FKName = "fk",
                ReferencedTable = "tbl1",
            };

            Assert.NotEqual(col1.GetHashCode(), col2.GetHashCode());
        }

        [FactWithName]
        public void TableHashIsOrderIndependent()
        {
            Column col1 = new Column()
            {
                ColumnName = "col1",
                TableName = "tbl1",
                TableSchema = "sch1",
                ColumnType = "type"
            };

            Column col2 = new Column()
            {
                ColumnName = "col2",
                TableName = "tbl1",
                TableSchema = "sch1",
                ColumnType = "type"
            };


            Table tbl1 = new Table();
            Table tbl2 = new Table();
            tbl1.Columns.AddRange(new List<Column> { col1, col2 });
            tbl2.Columns.AddRange(new List<Column> { col2, col1 });

            Assert.Equal(tbl1.GetHashCode(), tbl2.GetHashCode());
        }

        [FactWithName]
        public void DbSchemaBuilderBaseHashIsOrderIndependent()
        {
            Table tbl1 = new Table();
            Table tbl2 = new Table();

            tbl1.Columns.AddRange(new List<Column>
                {
                    new Column()
                    {
                        ColumnName = "col1",
                        TableName = "tbl1",
                        TableSchema = "sch1",
                        ColumnType = "type"
                    },
                    new Column()
                    {
                        ColumnName = "col2",
                        TableName = "tbl1",
                        TableSchema = "sch1",
                        ColumnType = "type"
                    }
                });
            tbl2.Columns.AddRange(new List<Column> {
                    new Column()
                    {
                        ColumnName = "col1",
                        TableName = "tbl1",
                        TableSchema = "sch1",
                        ColumnType = "type"
                    },
                    new Column()
                    {
                        ColumnName = "col1",
                        TableName = "tbl1",
                        TableSchema = "sch1",
                        ColumnType = "type"
                    }
                });

            var schema1 = new DbSchema();
            schema1.Tables.Add(tbl1);
            schema1.Tables.Add(tbl2);

            var schema2 = new DbSchema();
            schema2.Tables.Add(tbl2);
            schema2.Tables.Add(tbl1);


            Assert.Equal(schema1.GetHashCode(), schema2.GetHashCode());
        }
    }
}
