using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DynamicSchemaTableExtensionTests
    {
        private readonly DbSchema dbSchema = new DbSchema();

        [TestMethod]
        public void FindTable()
        {

            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            Table tbl = dbSchema.GetTable("db", "sch1", "name2");
            Assert.AreEqual("name2", tbl.Name);
        }

        [TestMethod]
        public void FindTableName()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            string tbl = dbSchema.GetTableName("db", "sch1", "name2");
            Assert.AreEqual("db_sch1_name2", tbl);
        }


        [TestMethod]
        [ExpectedException(typeof(TableNotFoundException))]
        public void FindTableThrowException()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });
            _ = dbSchema.GetTable("db", "sch1", "name21");
        }

        [TestMethod]
        public void FindTableOrNull()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            Table tbl = dbSchema.GetTableOrNull("db", "sch1", "name2");
            Assert.AreEqual("name2", tbl.Name);
        }

        [TestMethod]
        public void FindTableNameOrNull()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            string tbl = dbSchema.GetTableNameOrNull("db", "sch1", "name2");
            Assert.AreEqual("db_sch1_name2", tbl);
        }

        [TestMethod]
        public void FindTableOrNullDoesntThrowException()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            Table tbl = dbSchema.GetTableOrNull("db", "sch1", "name21");
            Assert.IsNull(tbl);
        }

        [TestMethod]
        public void FindTableNameReturnsNull()
        {
            dbSchema.Tables.Add(new Table { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Tables.Add(new Table { Name = "name2", Schema = "sch1", Database = "db" });

            string tbl = dbSchema.GetTableNameOrNull("db", "sch1", "name12");
            Assert.IsNull(tbl);
        }
    }
}