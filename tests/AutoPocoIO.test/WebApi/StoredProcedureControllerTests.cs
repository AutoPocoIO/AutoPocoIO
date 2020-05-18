using AutoPocoIO.Api;
using AutoPocoIO.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace AutoPocoIO.test.WebApi
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class StoredProcedureControllerTests : WebApiTestBase<IStoredProcedureOperations>
    {
        [TestMethod]
        public void GetCallsNoParams()
        {
            var obj = new { a = 1 };

            Ops.Setup(c => c.ExecuteNoParameters("conn", "proc", LoggingService))
                .Returns(obj);

            var controller = new StoredProcedureController(Ops.Object, LoggingService);

            var results = controller.Get("conn", "proc");
            Assert.AreEqual(1, results.a);
        }

        [TestMethod]
        public void PostCallsParams()
        {
            var obj = new { a = 1 };
            JObject jobject = new JObject
            {
                ["b"] = 1
            };

            Ops.Setup(c => c.Execute("conn", "proc", (JToken)jobject, LoggingService))
                .Returns(obj);

            var controller = new StoredProcedureController(Ops.Object, LoggingService);

            var results = controller.Post("conn", "proc", jobject);
            Assert.AreEqual(1, results.a);
        }
    }
}
