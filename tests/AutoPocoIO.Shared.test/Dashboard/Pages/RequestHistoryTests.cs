using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace AutoPocoIO.test.Dashboard.Pages
{
    [TestClass]
    public class RequestHistoryTests : PageTestBase
    {
        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void HistoryRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/Requests");

            var page = new Mock<RequestHistoryPage>(Mock.Of<IRequestHistoryRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.ListRequests()).Verifiable();

            Services.AddSingleton(page.Object);

            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void ListRequests()
        {
            var model = new List<RequestGridViewModel> { new RequestGridViewModel() { Connector = "conn1" } };
            var repo = new Mock<IRequestHistoryRepo>();
            repo.Setup(c => c.ListRequest(50)).Returns(model);

            var page = new RequestHistoryPage(repo.Object, Mock.Of<ILayoutPage>());
            page.ListRequests();

            Assert.AreEqual(model, page.ViewBag["requests"]);
        }
    }
}
