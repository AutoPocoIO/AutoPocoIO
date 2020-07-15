using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
    }
}
