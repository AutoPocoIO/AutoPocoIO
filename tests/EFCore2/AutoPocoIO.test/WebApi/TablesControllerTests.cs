﻿using AutoPocoIO.Api;
using AutoPocoIO.Extensions;
using AutoPocoIO.Services;
using AutoPocoIO.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.WebApi
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class TablesControllerTests : WebApiTestBase<ITableOperations>
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

            Ops.Setup(c => c.GetAll("conn", "tbl", LoggingService))
                .Returns((list, 14));

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
            CollectionAssert.AreEqual(list.ToList(), results.ToList());
        }

        [TestMethod]
        public void GetAllCallsGetAllOpUsesApplyQuery()
        {
            var list = new List<object> { new { a = 1 } }.AsQueryable();

            Ops.Setup(c => c.GetAll("conn", "tbl", LoggingService))
                .Returns((list, 0));

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            var results = controller.Get("conn", "tbl");
            CollectionAssert.AreEqual(new List<object>(), results.ToList());
        }

        [TestMethod]
        public void GetAllCallsGetByIdOp()
        {
            var obj = new { a = 1 };

            Ops.Setup(c => c.GetById("conn", "tbl", LoggingService, "14"))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            dynamic results = controller.Get("conn", "tbl", "14");
            Assert.AreEqual(1, results.a);
        }

        [TestMethod]
        public void PostCallsPostOp()
        {
            JToken obj = JObject.FromObject(new { a = 1 });

            Ops.Setup(c => c.CreateNewRow("conn", "tbl", obj, LoggingService))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            JToken results = (JToken)controller.Post("conn", "tbl", obj);
            dynamic resultsToObject = results.JTokenToConventionalDotNetObject();
            Assert.AreEqual(1, resultsToObject["a"]);
        }

        [TestMethod]
        public void PutCallsPutOp()
        {
            JToken obj = JObject.FromObject(new { a = 1 });

            Ops.Setup(c => c.UpdateRow("conn", "tbl", obj, LoggingService))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            JToken results = (JToken)controller.Put("conn", "tbl", obj);
            dynamic resultsToObject = results.JTokenToConventionalDotNetObject();
            Assert.AreEqual(1, resultsToObject["a"]);
        }
        [TestMethod]
        public void DeleteCallsDeleteOp()
        {
            var obj = new { a = 1 };

            Ops.Setup(c => c.DeleteRow("conn", "tbl",  LoggingService, "14"))
                .Returns(obj);

            var controller = new TablesController(Ops.Object, LoggingService, _queryStringService);

            dynamic results = controller.Delete("conn", "tbl", "14");
            Assert.AreEqual(1, results.a);
        }
    }
}
