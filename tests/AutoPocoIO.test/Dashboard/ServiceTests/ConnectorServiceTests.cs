using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AutoPocoIO.test.Dashboard.ServiceTests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class ConnectorServiceTests : DashboardServiceTests
    {
        [TestMethod]
        public void ListConnectorPageRegisteredDependencies()
        {
            var dashBoardProvider = new DashboardServiceProvider();
            provider = dashBoardProvider.GetServiceProvider(rootProvider);

            Assert.IsNotNull(provider.GetService<ConnectorsPage>());
            Assert.IsNotNull(provider.GetService<IConnectorRepo>());
        }

        [TestMethod]
        public void EditConnectorPageRegisteredDependencies()
        {
            var dashBoardProvider = new DashboardServiceProvider();
            provider = dashBoardProvider.GetServiceProvider(rootProvider);

            Assert.IsNotNull(provider.GetService<ConnectorForm>());
            Assert.IsNotNull(provider.GetService<IConnectorRepo>());
        }
    }
}
