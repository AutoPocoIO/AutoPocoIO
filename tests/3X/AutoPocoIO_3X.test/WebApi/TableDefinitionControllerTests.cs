using AutoPocoIO.Api;
using AutoPocoIO.Models;
using AutoPocoIO.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.WebApi
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class TableDefinitionControllerTests : WebApiTestBase<ITableOperations>
    {
        [TestMethod]
        public void GetDefinition()
        {
            var obj = new TableDefinition { Name = "tbl" };

            Ops.Setup(c => c.Definition("conn", "tbl", LoggingService))
                .Returns(obj);

            var controller = new TableDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "tbl");
            Assert.AreEqual("tbl", obj.Name);
        }

        [TestMethod]
        public void GetColumnDefinition()
        {
            var obj = new ColumnDefinition { Name = "col1" };

            Ops.Setup(c => c.Definition("conn", "tbl", "col1", LoggingService))
                .Returns(obj);

            var controller = new TableDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "tbl", "col1");
            Assert.AreEqual("col1", obj.Name);
        }
    }
}
