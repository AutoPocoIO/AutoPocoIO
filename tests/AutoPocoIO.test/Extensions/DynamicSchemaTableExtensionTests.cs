using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using System;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
    public class DynamicSchemaTableExtensionTests
    {
        private readonly DbSchema dbSchema = new DbSchema();

        [FactWithName]
        public void FindTable()
        {

            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            Table tbl = dbSchema.GetTable("db", "sch1", "name2");
            Assert.Equal("name2", tbl.Name);
        }

        [FactWithName]
        public void FindTableName()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            string tbl = dbSchema.GetTableName("db", "sch1", "name2");
            Assert.Equal("db_sch1_name2", tbl);
        }


        [FactWithName]
        public void FindTableThrowException()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });
             void act() => dbSchema.GetTable("db", "sch1", "name21");
            Assert.Throws<TableNotFoundException>(act);
        }

        [FactWithName]
        public void FindTableOrNull()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            Table tbl = dbSchema.GetTableOrNull("db", "sch1", "name2");
            Assert.Equal("name2", tbl.Name);
        }

        [FactWithName]
        public void FindTableNameOrNull()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            string tbl = dbSchema.GetTableNameOrNull("db", "sch1", "name2");
            Assert.Equal("db_sch1_name2", tbl);
        }

        [FactWithName]
        public void FindTableOrNullDoesntThrowException()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            Table tbl = dbSchema.GetTableOrNull("db", "sch1", "name21");
             Assert.Null(tbl);
        }

        [FactWithName]
        public void FindTableNameReturnsNull()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            string tbl = dbSchema.GetTableNameOrNull("db", "sch1", "name12");
             Assert.Null(tbl);
        }
    }
}