using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Middleware;
using AutoPocoIO.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Dashboard.Pages
{
    [TestClass]
    public class DataDictionaryTests : PageTestBase
    {
        Guid id = Guid.NewGuid();

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void DataDictionaryRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, "/DataDictionary");

            var page = new Mock<DataDictionaryPage>(Mock.Of<IConnectorRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.ListConnectors()).Verifiable();

            Services.AddSingleton(page.Object);

            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void DataDictionarySchemaRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, $"/DataDictionary/Schema/{id}");

            var page = new Mock<SchemaPage>(Mock.Of<IDataDictionaryRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.ListDbObjects(id)).Verifiable();

            Services.AddSingleton(page.Object);
            Context.UriMatch = dispatcher.Item2;
            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Integration)]
        public void DataDictionaryTableRoute()
        {
            var routes = new DashboardRoutes();
            var dispatcher = routes.Routes.FindDispatcher(Context, $"/DataDictionary/Table/{id}/tbl1");

            var page = new Mock<ObjectDetailsPage>(Mock.Of<IDataDictionaryRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.ListTableDetails(id, "tbl1")).Verifiable();

            Services.AddSingleton(page.Object);
            Context.UriMatch = dispatcher.Item2;
            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void ListConnectorsSetsViewBag()
        {
            var model = new List<ConnectorViewModel> { new ConnectorViewModel() { Id = id } };
            var repo = new Mock<IConnectorRepo>();
            repo.Setup(c => c.ListConnectors()).Returns(model);

            var page = new DataDictionaryPage(repo.Object, Mock.Of<ILayoutPage>());
            page.ListConnectors();

            Assert.AreEqual(model, page.ViewBag["Connectors"]);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void ListSchemaSetsViewBag()
        {
            var model = new SchemaViewModel() { ConnectorId = id };
            var repo = new Mock<IDataDictionaryRepo>();
            repo.Setup(c => c.ListSchemaObject(id)).Returns(model);

            var page = new SchemaPage(repo.Object, Mock.Of<ILayoutPage>());
            page.ListDbObjects(id);

            Assert.AreEqual(model, page.ViewBag["model"]);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void ListTableDetailsSetsViewBag()
        {
            var model = new TableDefinition() { ConnectorId = id };
            var navs = new List<NavigationPropertyViewModel> { new NavigationPropertyViewModel() { Name = "nav1" } };

            var repo = new Mock<IDataDictionaryRepo>();
            repo.Setup(c => c.ListTableDetails(id, "tbl")).Returns(model);
            repo.Setup(c => c.ListNavigationProperties(id, "tbl")).Returns(navs);

            var page = new ObjectDetailsPage(repo.Object, Mock.Of<ILayoutPage>());
            page.ListTableDetails(id, "tbl");

            Assert.AreEqual(model, page.ViewBag["model"]);
            Assert.AreEqual(navs, page.ViewBag["navs"]);
        }
    }
}
