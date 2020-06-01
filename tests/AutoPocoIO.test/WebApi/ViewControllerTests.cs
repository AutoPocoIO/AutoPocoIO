using AutoPocoIO.Api;
using AutoPocoIO.Services;
using AutoPocoIO.WebApi;
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.WebApi
{
    
     [Trait("Category", TestCategories.Unit)]
    public class ViewControllerTests : WebApiTestBase<IViewOperations>
    {
        private readonly IRequestQueryStringService _queryStringService;

        public ViewControllerTests()
        {
            var queryStringService = new Mock<IRequestQueryStringService>();
            queryStringService.Setup(c => c.GetQueryStrings())
                .Returns(new Dictionary<string, string>());
            _queryStringService = queryStringService.Object;
        }

        [FactWithName]
        public void GetAllCallsGetAllOp()
        {
            var list = new List<object> { new { a = 1 } }.AsQueryable();

            Ops.Setup(c => c.GetAllAndRecordLimit("conn", "tbl", LoggingService))
                .Returns((list, 14));

            var controller = new ViewsController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
             Assert.Equal(list.ToList(), results.ToList());
        }

        [FactWithName]
        public void GetAllCallsGetAllOpUsesApplyQuery()
        {
            var list = new List<object> { new { a = 1 } }.AsQueryable();

            Ops.Setup(c => c.GetAllAndRecordLimit("conn", "tbl", LoggingService))
                .Returns((list, 0));

            var controller = new ViewsController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
             Assert.Equal(new List<object>(), results.ToList());
        }
    }
}
