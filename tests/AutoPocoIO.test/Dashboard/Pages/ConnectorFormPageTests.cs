using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace AutoPocoIO.test.Dashboard.Pages
{
    [TestClass]
    public class ConnectorFormPageTests : PageTestBase
    {
        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void NewConnectorRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/Connectors/Connector/New");

            var page = new Mock<ConnectorForm>(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.NewConnector()).Verifiable();

            Services.AddSingleton(page.Object);

            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void GetConnectorRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/Connectors/Connector/123");

            var page = new Mock<ConnectorForm>(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.GetById("123")).Verifiable();

            Services.AddSingleton(page.Object);
            Context.UriMatch = dispatcher.Item2;
            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void SaveConnectorRoute()
        {
            SetupContext("post");

            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/Connectors/Connector/123");

            var page = new Mock<ConnectorForm>(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.Save()).Verifiable();
            page.Setup(c => c.SetForm(It.IsAny<IDictionary<string, string[]>>())).Verifiable();

            Services.AddSingleton(page.Object);
            Context.UriMatch = dispatcher.Item2;
            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }
    }
}
