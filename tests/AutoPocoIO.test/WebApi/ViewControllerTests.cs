using AutoPocoIO.Api;
using AutoPocoIO.Services;
using AutoPocoIO.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.WebApi
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ViewControllerTests : WebApiTestBase<IViewOperations>
    {
        private IRequestQueryStringService _queryStringService;

        [TestInitialize]
        public void Init()
        {
            var queryStringService = new Mock<IRequestQueryStringService>();
            queryStringService.Setup(c => c.GetQueryStrings())
                .Returns(new Dictionary<string, string>());
            _queryStringService = queryStringService.Object;
        }

        [TestMethod]
        public void GetAllCallsGetAllOp()
        {
            var list = new List<object> { new { a = 1 } }.AsQueryable();

            Ops.Setup(c => c.GetAllAndRecordLimit("conn", "tbl", LoggingService))
                .Returns((list, 14));

            var controller = new ViewsController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
            CollectionAssert.AreEqual(list.ToList(), results.ToList());
        }

        [TestMethod]
        public void GetAllCallsGetAllOpUsesApplyQuery()
        {
            var list = new List<object> { new { a = 1 } }.AsQueryable();

            Ops.Setup(c => c.GetAllAndRecordLimit("conn", "tbl", LoggingService))
                .Returns((list, 0));

            var controller = new ViewsController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
            CollectionAssert.AreEqual(new List<object>(), results.ToList());
        }
    }
}
