using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Dashboard.Pages
{
    [TestClass]
    public class ConnectorPageTests : PageTestBase
    {
        Guid id = Guid.NewGuid();

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void ConnectorsListRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/Connectors");

            var page = new Mock<ConnectorsPage>(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.ListConnectors()).Verifiable();

            Services.AddSingleton(page.Object);

            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void DeleteConnectorsRoute()
        {
            SetupContext("post");

            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, $"/Connectors/Delete/{id}");

            var page = new Mock<ConnectorsPage>(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.Delete(id)).Verifiable();

            Services.AddSingleton(page.Object);


            Context.UriMatch = dispatcher.Item2;
            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void ListConnectorsSetsViewBag()
        {
            var list = new List<ConnectorViewModel>() { new ConnectorViewModel() };
            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.ListConnectors())
                .Returns(list);

            var page = new ConnectorsPage(repo.Object, Mock.Of<ILayoutPage>());
            page.ListConnectors();

            Assert.AreEqual(list, page.ViewBag["Connectors"]);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void DeleteConnectorWithRepo()
        {
            var list = new List<ConnectorViewModel>() { new ConnectorViewModel() };
            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.Delete(id)).Verifiable();

            var page = new ConnectorsPage(repo.Object, Mock.Of<ILayoutPage>());
            page.Delete(id);

            repo.Verify();
        }


    }
}
