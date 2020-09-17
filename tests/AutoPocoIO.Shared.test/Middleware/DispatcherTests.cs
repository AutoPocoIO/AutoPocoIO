using AutoPocoIO.Dashboard;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoPocoIO.test.Middleware
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DispatcherTests
    {
        private class RazorPage1 : RazorPage, IRazorForm
        {
            public string result { get; set; }
            public IDictionary<string, string[]> formValues;

            public override void Execute()
            {
                WriteLiteral("outputHtml");
            }

            public IMiddlewareDispatcher Save()
            {
                result = "saved";
                return new RedirectDispatcher("test123", false);
            }

            public void SetForm(IDictionary<string, string[]> values)
            {
                formValues = values;
            }
        }

        IMiddlewareContext context;
        int statusCode;
        string location, content, responsebody;

        private void SetupContext(string method)
        {
            var request = new Mock<IMiddlewareRequest>();
            request.Setup(c => c.Method).Returns(method);
            request.Setup(c => c.PathBase).Returns("/basePath123");
            request.Setup(c => c.ReadFormAsync()).Returns(Task.FromResult<IDictionary<string, string[]>>(new Dictionary<string, string[]> { { "id", new[] { "12" } } }));

            var response = new Mock<IMiddlewareResponse>();
            response.SetupSet(c => c.StatusCode = It.IsAny<int>()).Callback<int>(c => statusCode = c);
            response.SetupSet(c => c.ContentType = It.IsAny<string>()).Callback<string>(c => content = c);
            response.Setup(c => c.Redirect(It.IsAny<string>())).Callback<string>(c => location = c);
            response.Setup(c => c.WriteAsync(It.IsAny<string>())).Callback<string>(c => responsebody = c);

            var services = new ServiceCollection();
            services.AddSingleton(new RazorPage1());

            var mockContext = new Mock<IMiddlewareContext>();
            mockContext.Setup(c => c.Request).Returns(request.Object);
            mockContext.Setup(c => c.Response).Returns(response.Object);
            mockContext.Setup(c => c.InternalServiceProvider).Returns(services.BuildServiceProvider());
            mockContext.Setup(c => c.UriMatch).Returns(Regex.Match("abc", "[a-z]*"));
            context = mockContext.Object;
        }
        [TestInitialize]
        public void Init()
        {
            SetupContext("get");
        }

        [TestMethod]
        public void ExecuteCommandRequiresPostDispatch()
        {
            var dispatcher = new CommandDispatcher<RazorPage1>((p, m) => p.result = "ran" + m.Value);
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();

            Assert.AreEqual(405, statusCode);
            Assert.AreEqual(null, context.InternalServiceProvider.GetService<RazorPage1>().result);
        }

        [TestMethod]
        public void ExecuteCommandOnDispatch()
        {
            SetupContext("post");

            var dispatcher = new CommandDispatcher<RazorPage1>((p, m) => p.result = "ran" + m.Value);
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();

            Assert.AreEqual(200, statusCode);

            var page = context.InternalServiceProvider.GetService<RazorPage1>();
            Assert.AreEqual("ranabc", page.result);
            Assert.IsNotNull(page.LoggingService);

        }

        [TestMethod]
        public void ExecuteFormRequiresPostDispatch()
        {
            var dispatcher = new FormDispatcher<RazorPage1>();
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();

            Assert.AreEqual(405, statusCode);
            Assert.AreEqual(null, context.InternalServiceProvider.GetService<RazorPage1>().result);
        }

        [TestMethod]
        public void ExecuteFormSaveOnDispatch()
        {
            SetupContext("post");

            var dispatcher = new FormDispatcher<RazorPage1>();
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();


            var page = context.InternalServiceProvider.GetService<RazorPage1>();

            Assert.AreEqual("test123", location);  //returned dispatcher
            Assert.AreEqual("saved", page.result); //saved
            Assert.AreEqual("12", page.formValues["id"][0]); //set form
            Assert.IsNotNull(page.LoggingService);
        }

        [TestMethod]
        public void PageDispatcherReturnsPageHtml()
        {
            var page = new RazorPage1();
            var dispatcher = new RazorPageDispatcher(page);
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();

            Assert.AreEqual("text/html", content);
            Assert.AreEqual("outputHtml", responsebody);
        }

        [TestMethod]
        public void PageDispatcherTReturnsPageHtml()
        {
            var dispatcher = new RazorPageDispatcher<RazorPage1>((p, m) => p.result = "page" + m.Value);
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();

            Assert.AreEqual("text/html", content);
            Assert.AreEqual("outputHtml", responsebody);
            var page = context.InternalServiceProvider.GetService<RazorPage1>();
            Assert.IsNotNull(page.LoggingService);
            Assert.AreEqual("pageabc", page.result);
        }

        [TestMethod]
        public void RedirectDispatcherWithBasePath()
        {
            var dispatcher = new RedirectDispatcher("newLocation");
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();

            Assert.AreEqual("/basePath123/newLocation", location);
        }

        [TestMethod]
        public void RedirectDispatcherWithoutBasePath()
        {
            var dispatcher = new RedirectDispatcher("newLocation", false);
            dispatcher.Dispatch(context, Mock.Of<ILoggingService>()).Wait();

            Assert.AreEqual("newLocation", location);
        }
    }
}
