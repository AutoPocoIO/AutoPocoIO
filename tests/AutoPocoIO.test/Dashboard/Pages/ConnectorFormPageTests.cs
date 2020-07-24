using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

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

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void NewConnectorIsActive()
        {
            var page = new ConnectorForm(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.NewConnector();

            Assert.IsTrue(((ConnectorViewModel)page.ViewBag["model"]).IsActive);
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void SetFormValuesMapping()
        {
            var formValues = new Dictionary<string, string[]>
            {
                {"id", new[]{"1234"} },
                {"connectorName", new[]{"name1"} },
                {"serverName", new[]{"serv"} },
                {"databaseName", new[]{"db1"} },
                {"schema", new[]{"sch1"} },
                {"userId", new[]{"id1"} },
                {"password", new[]{"pass1"} },
                {"recordLimit", new[]{"100"} },
                {"isEnabled", new[]{"true"} },
            };
            var page = new ConnectorForm(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.SetForm(formValues);

            var model = (ConnectorViewModel)new PrivateObject(page).GetField("model");

            Assert.AreEqual("1234", model.Id);
            Assert.AreEqual(1, model.ResourceType);
            Assert.AreEqual("name1", model.Name);
            Assert.AreEqual("serv", model.DataSource);
            Assert.AreEqual("db1", model.InitialCatalog);
            Assert.AreEqual("sch1", model.Schema);
            Assert.AreEqual("id1", model.UserId);
            Assert.AreEqual("pass1", model.Password);
            Assert.AreEqual(100, model.RecordLimit);
            Assert.AreEqual(true, model.IsActive);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void SetViewBagToConnectorById()
        {
            var connector = new ConnectorViewModel { Id = "123", Name = "abc" };

            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.GetById("123")).Returns(connector);
            var page = new ConnectorForm(repo.Object, Mock.Of<ILayoutPage>());
            page.GetById("123");

            Assert.AreEqual(connector, page.ViewBag["model"]);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void ErrorSetsViewBagAndReturnsPage()
        {
            var connector = new ConnectorViewModel { Id = "123", Name = "abc" };
            var errors = new Dictionary<string, string>() { { "error", "val" } };

            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.Validate(connector, errors)).Verifiable();

            var page = new ConnectorForm(repo.Object, Mock.Of<ILayoutPage>());
            page.LoggingService = Logging;

            var privateObj = new PrivateObject(page);
            privateObj.SetField("model", connector);
            privateObj.SetField("errors", errors);

            var dispatcher = page.Save();

            Assert.AreEqual(0, Logging.LogCount);
            Assert.IsInstanceOfType(dispatcher, typeof(RazorPageDispatcher));
            Assert.AreEqual(page, new PrivateObject(dispatcher).GetField("_page"));
            Assert.AreEqual(connector, page.ViewBag["model"]);

            repo.Verify(c => c.Insert(It.IsAny<ConnectorViewModel>()), Times.Never);
            repo.Verify(c => c.Save(It.IsAny<ConnectorViewModel>()), Times.Never);
            repo.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void InsertLogsAndRedirects()
        {
            var connector = new ConnectorViewModel { Name = "abc" };

            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.Insert(connector)).Returns("123");
            repo.Setup(c => c.Validate(connector, new Dictionary<string, string>())).Verifiable();

            var page = new ConnectorForm(repo.Object, Mock.Of<ILayoutPage>());
            page.LoggingService = Logging;

            var privateObj = new PrivateObject(page);
            privateObj.SetField("model", connector);

            var dispatcher = page.Save();

            Assert.AreEqual(1, Logging.LogCount);
            Assert.AreEqual("POST", Logging.PublicRequests.First().RequestType);
            Assert.IsInstanceOfType(dispatcher, typeof(RedirectDispatcher));
            Assert.AreEqual("/Connectors/Connector/123", new PrivateObject(dispatcher).GetField("_location"));
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void UpdateLogsAndRedirects()
        {
            var connector = new ConnectorViewModel { Id = "123", Name = "abc" };

            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.Save(connector)).Returns("123");
            repo.Setup(c => c.Validate(connector, new Dictionary<string, string>())).Verifiable();

            var page = new ConnectorForm(repo.Object, Mock.Of<ILayoutPage>());
            page.LoggingService = Logging;

            var privateObj = new PrivateObject(page);
            privateObj.SetField("model", connector);

            var dispatcher = page.Save();

            Assert.AreEqual(1, Logging.LogCount);
            Assert.AreEqual("PUT", Logging.PublicRequests.First().RequestType);
            Assert.IsInstanceOfType(dispatcher, typeof(RedirectDispatcher));
            Assert.AreEqual("/Connectors/Connector/123", new PrivateObject(dispatcher).GetField("_location"));
        }


    }
}
