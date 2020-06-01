using AutoPocoIO.Api;
using AutoPocoIO.Models;
using AutoPocoIO.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.WebApi
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class StoredProcedureParameterDefinitionTests : WebApiTestBase<IStoredProcedureOperations>
    {
        [TestMethod]
        public void GetDefinition()
        {
            var obj = new StoredProcedureDefinition { Name = "proc" };

            Ops.Setup(c => c.Definition("conn", "proc", LoggingService))
                .Returns(obj);

            var controller = new StoredProcedureDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "proc");
            Assert.AreEqual("proc", obj.Name);
        }

        [TestMethod]
        public void GetParameterDefinition()
        {
            var obj = new StoredProcedureParameterDefinition { Name = "param1" };

            Ops.Setup(c => c.Definition("conn", "proc", "param1", LoggingService))
                .Returns(obj);

            var controller = new StoredProcedureDefinitionController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "proc", "param1");
            Assert.AreEqual("param1", obj.Name);
        }
    }
}
