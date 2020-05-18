using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.MsSql.DynamicSchema.Db;
using AutoPocoIO.Resources;
using AutoPocoIO.test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.MsSql.test.DynamicSchema.Db
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class MsSqlConnectionBuilderTests
    {
        [TestMethod]
        public void ResourceTypeSet()
        {
            var builder = new MsSqlConnectionBuilder();
            Assert.AreEqual(ResourceType.Mssql, builder.ResourceType);
        }

        [TestMethod]
        public void CreateConnectionString()
        {
            var info = new ConnectionInformation()
            {
                InitialCatalog = "cat1",
                UserId = "user1",
                DataSource = "dt1",
                Password = "pass1",
            };

            var builder = new MsSqlConnectionBuilder();
            var results = builder.CreateConnectionString(info);

            Assert.AreEqual("Data Source=dt1;Initial Catalog=cat1;Persist Security Info=False;User ID=user1;Password=pass1;MultipleActiveResultSets=False;Connect Timeout=30;TrustServerCertificate=False", results);
        }

        [TestMethod]
        public void ParseConnectionString()
        {
            var connString = "Data Source=dt1;Initial Catalog=cat1;Persist Security Info=False;User ID=user1;Password=pass1;MultipleActiveResultSets=False;Connect Timeout=30;TrustServerCertificate=False";
            var builder = new MsSqlConnectionBuilder();
            var results = builder.ParseConnectionString(connString);

            Assert.AreEqual("cat1", results.InitialCatalog);
            Assert.AreEqual("user1", results.UserId);
            Assert.AreEqual("dt1", results.DataSource);
            Assert.AreEqual("pass1", results.Password);


        }
    }
}
