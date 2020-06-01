using AutoPocoIO.Api;
using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using AutoPocoIO.WebApi;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.WebApi
{
    [Trait("Category", TestCategories.Unit)]
    public class TablesControllerTests : WebApiTestBase<ITableOperations>
    {
        private readonly IRequestQueryStringService _queryStringService;

        public TablesControllerTests()
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

            Ops.Setup(c => c.GetAll("conn", "tbl", LoggingService))
                .Returns((list, 14));

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
             Assert.Equal(list.ToList(), results.ToList());
        }

        [FactWithName]
        public void GetAllCallsGetAllOpUsesApplyQuery()
        {
            var list = new List<object> { new { a = 1 } }.AsQueryable();

            Ops.Setup(c => c.GetAll("conn", "tbl", LoggingService))
                .Returns((list, 0));

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
             Assert.Equal(new List<object>(), results.ToList());
        }

        [FactWithName]
        public void GetAllCallsGetByIdOp()
        {
            var obj = new { a = 1 };

            Ops.Setup(c => c.GetById("conn", "tbl", "14", LoggingService))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            dynamic results = controller.Get("conn", "tbl", "14");
            Assert.Equal(1, results.a);
        }

        [FactWithName]
        public void PostCallsPostOp()
        {
            JToken obj = JObject.FromObject(new { a = 1 });

            Ops.Setup(c => c.CreateNewRow("conn", "tbl", obj, LoggingService))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            JToken results = (JToken)controller.Post("conn", "tbl", obj);
            dynamic resultsToObject = results.JTokenToConventionalDotNetObject();
            Assert.Equal(1, resultsToObject["a"]);
        }

        [FactWithName]
        public void PutCallsPutOp()
        {
            JToken obj = JObject.FromObject(new { a = 1 });

            Ops.Setup(c => c.UpdateRow("conn", "tbl", "14", obj, LoggingService))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            JToken results = (JToken)controller.Put("conn", "tbl", "14", obj);
            dynamic resultsToObject = results.JTokenToConventionalDotNetObject();
            Assert.Equal(1, resultsToObject["a"]);
        }
        [FactWithName]
        public void DeleteCallsDeleteOp()
        {
            var obj = new { a = 1 };

            Ops.Setup(c => c.DeleteRow("conn", "tbl", "14", LoggingService))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            dynamic results = controller.Delete("conn", "tbl", "14");
            Assert.Equal(1, results.a);
        }
    }
}
