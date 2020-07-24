using AutoPocoIO.Extensions;
using AutoPocoIO.test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.MsSql.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class StoredProcedureExtensionsTests
    {
        private class Test1
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        [TestMethod]
        public void GetResultSetNull()
        {
            var output = new Dictionary<string, object>
            {
                { "ResultSet", new List< Dictionary<string, object>>
                  {
                    new Dictionary<string, object>
                    {
                        { "id", 1 },
                        {"name" , "name1" }
                    } } },
                { "Other", null }
            };

            var results = output.ProjectMsSqlResultSet<Test1>();

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.First().id);
            Assert.AreEqual("name1", results.First().name);
        }

        [TestMethod]
        public void GetResultSetZero()
        {
            var output = new Dictionary<string, object>
            {
                { "ResultSet", new List< Dictionary<string, object>>
                  {
                    new Dictionary<string, object>
                    {
                        { "id", 1 },
                        {"name" , "name1" }
                    } } },
                { "Other", null }
            };

            var results = output.ProjectMsSqlResultSet<Test1>(0);

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.First().id);
            Assert.AreEqual("name1", results.First().name);
        }
    }
}
