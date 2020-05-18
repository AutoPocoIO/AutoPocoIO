using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Runtime
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
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




        [TestMethod]
        public void PropertyNameFound()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnTestClass), new Dictionary<string, string>());
            Assert.AreEqual(2, properties.Count());
            Assert.AreEqual("Test", properties.First());
            Assert.AreEqual("Int32?(null) as TestExclude", properties.Last());
        }

        [TestMethod]
        public void PropertyNameFoundWithExpand()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnTestClass), new Dictionary<string, string>() { { "$expand", "TestExclude" } });
            Assert.AreEqual(2, properties.Count());
            Assert.AreEqual("Test", properties.First());
            Assert.AreEqual("TestExclude", properties.Last());
        }


        [TestMethod]
        public void PropertyNameFoundWithPrefix()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnTestClass), new Dictionary<string, string>(), "testPre.");

            Assert.AreEqual(2, properties.Count());
            Assert.AreEqual("testPre.Test", properties.First());
            Assert.AreEqual("Int32?(null) as TestExclude", properties.Last());
        }

        [TestMethod]
        public void ProperNameExcludeObjects()
        {
            Table tbl = new Table();
            tbl.Columns.Add(new Column { ColumnName = "Test" });

            IEnumerable<string> properties = ReflectionExtensions.UserJoinedColumnSelect(tbl, typeof(UserJoinColumnWithObjectTypeTestClass), new Dictionary<string, string>());
            Assert.AreEqual(1, properties.Count());
            Assert.AreEqual("Test", properties.First());
        }
    }
}
