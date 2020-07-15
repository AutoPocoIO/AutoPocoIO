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
    public class DataDictionaryTests : PageTestBase
    {
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
            var dispatcher = routes.Routes.FindDispatcher(Context, "/DataDictionary/Schema/123");

            var page = new Mock<SchemaPage>(Mock.Of<IDataDictionaryRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.ListDbObjects("123")).Verifiable();

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
            var dispatcher = routes.Routes.FindDispatcher(Context, "/DataDictionary/Table/123/tbl1");

            var page = new Mock<ObjectDetailsPage>(Mock.Of<IDataDictionaryRepo>(), Mock.Of<ILayoutPage>());
            page.Setup(c => c.ListTableDetails("123", "tbl1")).Verifiable();

            Services.AddSingleton(page.Object);
            Context.UriMatch = dispatcher.Item2;
            dispatcher.Item1.Dispatch(Context, Logging);

            page.Verify();
        }

  
    }
}
