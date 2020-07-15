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
    public class DashboardPageTests : PageTestBase
    {
        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void DailyDashboardRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/");

            var page = new Mock<DashboardPage>(Mock.Of<IDashboardRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.DailyStats()).Verifiable();

            Services.AddSingleton(page.Object);

            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void WeeklyDashboardRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/Weekly");

            var page = new Mock<DashboardPage>(Mock.Of<IDashboardRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.WeeklyStats()).Verifiable();

            Services.AddSingleton(page.Object);

            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }
    }
}
