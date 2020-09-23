using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using AutoPocoIO.test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.Auth.test.DynamicSchema
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class SchemaInitializerTests
    {
        [TestMethod]
        public void ConfgiureAction()
        {
            var config = new Config();

            var schemaBuilder = new Mock<IDbSchemaBuilder>();

            var schmea = new SchemaInitializer(config,
             schemaBuilder.Object, Mock.Of<IDbSchema>());

            schmea.ConfigureAction(new Connector { ConnectionStringDecrypted = "conn1" }, OperationType.read);
            Assert.AreEqual("conn1", config.ConnectionString);
        }

        [TestMethod]
        public void InitializeTable()
        {
            var config = new Config
            {
                IncludedTable = "tbl"
            };

            var schemaBuilder = new Mock<IDbSchemaBuilder>();

            var schmea = new SchemaInitializer(config,
             schemaBuilder.Object, Mock.Of<IDbSchema>()
            );

            schmea.Initilize();

            schemaBuilder.Verify(c => c.GetColumns(), Times.Once);
        }

        [TestMethod]
        public void InitializeSchema()
        {
            var config = new Config();

            var schemaBuilder = new Mock<IDbSchemaBuilder>();

            var schmea = new SchemaInitializer(config,
             schemaBuilder.Object, Mock.Of<IDbSchema>());

            schmea.Initilize();

            schemaBuilder.Verify(c => c.GetTableViews(), Times.Once);
            schemaBuilder.Verify(c => c.GetStoredProcedures(), Times.Once);
        }
    }
}
