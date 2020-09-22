using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public partial class SchmeaBuilderBaseStoredProcedure
    {
        private Config config;
        private SchemaBuilder1 builder;
        private DbSchema schema;

        [TestInitialize]
        public void Init()
        {
            config = new Config();
            schema = new DbSchema();

            builder = new SchemaBuilder1(config, schema, new DbTypeMapper())
            {
                DtProcs = SchmeaBuilderDataTableBuilder.CreatProcTable()
            };
        }

        [TestMethod]
        public void ListProcs()
        {
            var row = builder.DtProcs.NewRow();
            row["ProcSchema"] = "sch1";
            row["ProcName"] = "proc1";
            row["DatabaseName"] = "db1";
            builder.DtProcs.Rows.Add(row);

            builder.GetStoredProcedures();

            Assert.AreEqual(1, schema.StoredProcedures.Count());
            var proc = schema.StoredProcedures.First();
            Assert.AreEqual("sch1", proc.Schema);
            Assert.AreEqual("proc1", proc.Name);
            Assert.AreEqual("db1", proc.Database);
        }

        [TestMethod]
        public void ListProcs2()
        {
            var row = builder.DtProcs.NewRow();
            row["ProcSchema"] = "sch1";
            row["ProcName"] = "proc1";
            row["DatabaseName"] = "db1";
            builder.DtProcs.Rows.Add(row);

            row = builder.DtProcs.NewRow();
            row["ProcSchema"] = "sch1";
            row["ProcName"] = "proc2";
            row["DatabaseName"] = "db1";
            builder.DtProcs.Rows.Add(row);

            builder.GetStoredProcedures();

            Assert.AreEqual(2, schema.StoredProcedures.Count());
        }

        [TestMethod]
        public void ListProcParams()
        {
            var row = builder.DtProcs.NewRow();
            row["ProcSchema"] = "sch1";
            row["ProcName"] = "proc1";
            row["DatabaseName"] = "db1";
            row["ParamName"] = "param1";
            row["ParamType"] = "varchar";
            row["IsOutput"] = "true";
            row["IsNullable"] = "false";
            builder.DtProcs.Rows.Add(row);

            row = builder.DtProcs.NewRow();
            row["ProcSchema"] = "sch1";
            row["ProcName"] = "proc1";
            row["DatabaseName"] = "db1";
            row["ParamName"] = "param2";
            row["ParamType"] = "int";
            row["IsOutput"] = "true";
            row["IsNullable"] = "false";
            builder.DtProcs.Rows.Add(row);

            builder.GetStoredProcedures();

            Assert.AreEqual(1, schema.StoredProcedures.Count());
            var params1 = schema.StoredProcedures.First().Parameters;
            Assert.AreEqual(2, schema.StoredProcedures.First().Parameters.Count());
            Assert.AreEqual("param1", params1.First().Name);
            Assert.AreEqual("param2", params1.Last().Name);
            Assert.AreEqual("varchar", params1.First().Type);
            Assert.AreEqual(true, params1.First().IsOutput);
            Assert.AreEqual(false, params1.First().IsNullable);

        }
    }
}
