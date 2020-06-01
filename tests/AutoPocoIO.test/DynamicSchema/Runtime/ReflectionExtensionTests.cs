using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Runtime
{
    
     [Trait("Category", TestCategories.Unit)]
    public class ReflectionExtensionTests
    {
        private class UserJoinColumnTestClass
        {
            public static string Test { get; set; }
            public static string TestExclude { get; set; }
        }

        private static class UserJoinColumnWithObjectTypeTestClass
        {
            public static string Test { get; set; }
            public static object TestExclude { get; set; }
        }

        private static class UserJoinColumnWithVEypeTestClass
        {
            public static string Test { get; set; }
            public static UserJoinColumnTestClass VEToVE_Exclude { get; set; }
            public static UserJoinColumnTestClass VEToBase_Exclude { get; set; }
        }




        [FactWithName]
        public void PropertyNameFound()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnTestClass), new Dictionary<string, string>());
            Assert.Equal(2, properties.Count());
            Assert.Equal("Test", properties.First());
            Assert.Equal("Int32?(null) as TestExclude", properties.Last());
        }

        [FactWithName]
        public void PropertyNameFoundWithExpand()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnTestClass), new Dictionary<string, string>() { { "$expand", "TestExclude" } });
            Assert.Equal(2, properties.Count());
            Assert.Equal("Test", properties.First());
            Assert.Equal("TestExclude", properties.Last());
        }


        [FactWithName]
        public void PropertyNameFoundWithPrefix()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnTestClass), new Dictionary<string, string>(), "testPre.");

            Assert.Equal(2, properties.Count());
            Assert.Equal("testPre.Test", properties.First());
            Assert.Equal("Int32?(null) as TestExclude", properties.Last());
        }

        [FactWithName]
        public void ProperNameExcludeObjects()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnWithObjectTypeTestClass), new Dictionary<string, string>());
            Assert.Single(properties);
            Assert.Equal("Test", properties.First());
        }
    }
}
