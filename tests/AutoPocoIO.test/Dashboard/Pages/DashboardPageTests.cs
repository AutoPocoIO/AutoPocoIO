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

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void SetViewBagDailyStats()
        {
            var repo = new Mock<IDashboardRepo>();
            repo.Setup(c => c.HourlyRequest()).Returns(new System.Tuple<string[], int[], int[]>(new[] { "vals" }, new[] { 1 }, new[] { 2 }));
            repo.Setup(c => c.TotalRequests(0)).Returns(11);
            repo.Setup(c => c.SuccessFullRequests(0)).Returns(12);
            repo.Setup(c => c.FailedRequests(0)).Returns(13);
            repo.Setup(c => c.UnauthorizedRequest(0)).Returns(14);
            repo.Setup(c => c.TotalRequestsTime(0)).Returns(15);
            repo.Setup(c => c.SuccessFullRequestsTime(0)).Returns(16);
            repo.Setup(c => c.FailedRequestsTime(0)).Returns(17);
            repo.Setup(c => c.UnauthorizedRequestTime(0)).Returns(18);

            var page = new DashboardPage(repo.Object, Mock.Of<ILayoutPage>());
            page.DailyStats();

            Assert.AreEqual("", page.ViewBag["IsWeekly"]);
            Assert.AreEqual("active", page.ViewBag["IsDaily"]);

            CollectionAssert.AreEqual(new[] { "vals" }, (string[]) page.ViewBag["GraphLabels"]);
            CollectionAssert.AreEqual(new[] { 1 }, (int[])page.ViewBag["SuccessfulGraph"]);
            CollectionAssert.AreEqual(new[] { 2 }, (int[])page.ViewBag["FailGraph"]);

            Assert.AreEqual(11, page.ViewBag["TotalCount"]);
            Assert.AreEqual(12, page.ViewBag["SuccessfulCount"]);
            Assert.AreEqual(13, page.ViewBag["FailCount"]);
            Assert.AreEqual(14, page.ViewBag["UnauthorizedCount"]);

            Assert.AreEqual(15, page.ViewBag["TotalTime"]);
            Assert.AreEqual(16, page.ViewBag["SuccessfulTime"]);
            Assert.AreEqual(17, page.ViewBag["FailCountTime"]);
            Assert.AreEqual(18, page.ViewBag["UnauthorizedTime"]);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void SetViewBagWeeklyStats()
        {
            var repo = new Mock<IDashboardRepo>();
            repo.Setup(c => c.WeeklyRequest()).Returns(new System.Tuple<string[], int[], int[]>(new[] { "vals" }, new[] { 1 }, new[] { 2 }));
            repo.Setup(c => c.TotalRequests(-6)).Returns(11);
            repo.Setup(c => c.SuccessFullRequests(-6)).Returns(12);
            repo.Setup(c => c.FailedRequests(-6)).Returns(13);
            repo.Setup(c => c.UnauthorizedRequest(-6)).Returns(14);
            repo.Setup(c => c.TotalRequestsTime(-6)).Returns(15);
            repo.Setup(c => c.SuccessFullRequestsTime(-6)).Returns(16);
            repo.Setup(c => c.FailedRequestsTime(-6)).Returns(17);
            repo.Setup(c => c.UnauthorizedRequestTime(-6)).Returns(18);

            var page = new DashboardPage(repo.Object, Mock.Of<ILayoutPage>());
            page.WeeklyStats();

            Assert.AreEqual("active", page.ViewBag["IsWeekly"]);
            Assert.AreEqual("", page.ViewBag["IsDaily"]);

            CollectionAssert.AreEqual(new[] { "vals" }, (string[])page.ViewBag["GraphLabels"]);
            CollectionAssert.AreEqual(new[] { 1 }, (int[])page.ViewBag["SuccessfulGraph"]);
            CollectionAssert.AreEqual(new[] { 2 }, (int[])page.ViewBag["FailGraph"]);

            Assert.AreEqual(11, page.ViewBag["TotalCount"]);
            Assert.AreEqual(12, page.ViewBag["SuccessfulCount"]);
            Assert.AreEqual(13, page.ViewBag["FailCount"]);
            Assert.AreEqual(14, page.ViewBag["UnauthorizedCount"]);

            Assert.AreEqual(15, page.ViewBag["TotalTime"]);
            Assert.AreEqual(16, page.ViewBag["SuccessfulTime"]);
            Assert.AreEqual(17, page.ViewBag["FailCountTime"]);
            Assert.AreEqual(18, page.ViewBag["UnauthorizedTime"]);
        }
    }
}
