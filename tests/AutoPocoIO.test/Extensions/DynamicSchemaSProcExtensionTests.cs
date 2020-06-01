using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using System;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    [Trait("Category", TestCategories.Unit)]
    public class DynamicSchemaSprocExtensionTests
    {
        private readonly DbSchema dbSchema;
        public DynamicSchemaSprocExtensionTests()
        {
            dbSchema = new DbSchema();
        }

        [FactWithName]
        public void FindProc()
        {
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name1", Schema = "sch1" });
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name2", Schema = "sch1" });

            var tbl = dbSchema.GetStoredProcedure("sch1", "name2");
            Assert.Equal("name2", tbl.Name);
        }

        [FactWithName]
        public void FindProchrowException()
        {
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name1", Schema = "sch1" });
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name2", Schema = "sch1" });
             void act() => dbSchema.GetStoredProcedure("sch1", "name12");
            Assert.Throws<StoreProcedureNotFoundException>(act);
        }

        [FactWithName]
        public void FindProcDifferentSchemaThrowException()
        {
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name1", Schema = "sch1" });
            dbSchema.StoredProcedures.Add(new StoredProcedure { Name = "name2", Schema = "sch1" });
             void act() => dbSchema.GetStoredProcedure("sch2", "name1");
            Assert.Throws<StoreProcedureNotFoundException>(act);
        }
    }
}
