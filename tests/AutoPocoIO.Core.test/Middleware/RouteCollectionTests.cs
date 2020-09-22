using AutoPocoIO.Constants;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoPocoIO.test.Middleware
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class RouteCollectionTests
    {
        RouteCollection routes = new RouteCollection();
        IMiddlewareContext context;
        [TestInitialize]
        public void Init()
        {
            var request = new Mock<IMiddlewareRequest>();
            request.Setup(c => c.Method).Returns("get");

            var mockContext = new Mock<IMiddlewareContext>();
            mockContext.Setup(c => c.Request).Returns(request.Object);

            context = mockContext.Object;
        }


        [TestMethod]
        public void FindRootRoute()
        {
            var dispatcher = new Mock<IMiddlewareDispatcher>();
            routes.Add("/", HttpMethodType.GET, dispatcher.Object);

            var result = routes.FindDispatcher(context, "");
            Assert.AreEqual(dispatcher.Object, result.Item1);
        }

        [TestMethod]
        public void FindRootSlashRoute()
        {
            var dispatcher = new Mock<IMiddlewareDispatcher>();
            routes.Add("/", HttpMethodType.GET, dispatcher.Object);

            var result = routes.FindDispatcher(context, "/");
            Assert.AreEqual(dispatcher.Object, result.Item1);
        }

        [TestMethod]
        public void FindGroupValueInRoute()
        {
            var dispatcher = new Mock<IMiddlewareDispatcher>();
            routes.Add("/test/(?<id>.+)", HttpMethodType.GET, dispatcher.Object);

            var result = routes.FindDispatcher(context, "/test/abc");
            Assert.AreEqual(dispatcher.Object, result.Item1);
            Assert.AreEqual("abc", result.Item2.Groups["id"].Value);
        }

        [TestMethod]
        public void RouteNotFound()
        {
            var dispatcher = new Mock<IMiddlewareDispatcher>();
            routes.Add("/test/", HttpMethodType.GET, dispatcher.Object);

            var result = routes.FindDispatcher(context, "/otherRoute");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouteTypeMismatch()
        {
            int statusCode = 0;

            var request = new Mock<IMiddlewareRequest>();
            request.Setup(c => c.Method).Returns("post");
            var response = new Mock<IMiddlewareResponse>();
            response.SetupSet(c => c.StatusCode = It.IsAny<int>()).Callback<int>(c => statusCode = c);

            var mockContext = new Mock<IMiddlewareContext>();
            mockContext.Setup(c => c.Request).Returns(request.Object);
            mockContext.Setup(c => c.Response).Returns(response.Object);
            context = mockContext.Object;

            routes.Add("/test", HttpMethodType.GET, Mock.Of<IMiddlewareDispatcher>());

            var result = routes.FindDispatcher(context, "/test");
            Assert.IsNull(result);
            Assert.AreEqual(405, statusCode);
        }
    }
}
