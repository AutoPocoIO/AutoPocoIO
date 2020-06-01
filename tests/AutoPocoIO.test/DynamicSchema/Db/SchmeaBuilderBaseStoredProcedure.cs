using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Xunit;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    [Trait("Category", TestCategories.Unit)]
    public partial class SchmeaBuilderBaseStoredProcedure
    {
        private readonly Config config;
        private readonly SchemaBuilder1 builder;
        private readonly DbSchema schema;

        public SchmeaBuilderBaseStoredProcedure()
        {
            config = new Config();
            schema = new DbSchema();

            builder = new SchemaBuilder1(config, schema, new DbTypeMapper())
            {
                DtProcs = SchmeaBuilderDataTableBuilder.CreatProcTable()
            };
        }

        [FactWithName]
        public void ListProcs()
        {
            var row = builder.DtProcs.NewRow();
            row["ProcSchema"] = "sch1";
            row["ProcName"] = "proc1";
            row["DatabaseName"] = "db1";
            builder.DtProcs.Rows.Add(row);

            builder.GetStoredProcedures();

            Assert.Single(schema.StoredProcedures);
            var proc = schema.StoredProcedures.First();
            Assert.Equal("sch1", proc.Schema);
            Assert.Equal("proc1", proc.Name);
            Assert.Equal("db1", proc.Database);
        }

        [FactWithName]
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

            Assert.Equal(2, schema.StoredProcedures.Count());
        }

        [FactWithName]
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

            Assert.Single(schema.StoredProcedures);
            var params1 = schema.StoredProcedures.First().Parameters;
            Assert.Equal(2, schema.StoredProcedures.First().Parameters.Count());
            Assert.Equal("param1", params1.First().Name);
            Assert.Equal("param2", params1.Last().Name);
            Assert.Equal("varchar", params1.First().Type);
            Assert.True(params1.First().IsOutput);
            Assert.False(params1.First().IsNullable);

        }
    }
}
