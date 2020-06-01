using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using System;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class DynamicSchemaViewExtensionTests
    {
        private readonly DbSchema dbSchema = new DbSchema();

        [FactWithName]
        public void FindView()
        {
            dbSchema.Views.Add(new View { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Views.Add(new View { Name = "name2", Schema = "sch1", Database = "db" });

            View tbl = dbSchema.GetView("sch1", "name2");
            Assert.Equal("name2", tbl.Name);
        }

        [FactWithName]
        public void FindViewThrowException()
        {
            dbSchema.Views.Add(new View { Name = "name1", Schema = "sch1", Database = "db" });
            dbSchema.Views.Add(new View { Name = "name2", Schema = "sch1", Database = "db" });
             void act() => dbSchema.GetView("sch1", "name21");
            Assert.Throws<ViewNotFoundException>(act);
        }
    }
}
