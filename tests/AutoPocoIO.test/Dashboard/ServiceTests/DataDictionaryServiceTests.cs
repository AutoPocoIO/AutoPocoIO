using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Dashboard.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AutoPocoIO.test.Dashboard.ServiceTests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class DataDictionaryServiceTests: DashboardServiceTests
    {
        [TestMethod]
        public void DataDictionaryPageRegisteredDependencies()
        {
            var dashBoardProvider = new DashboardServiceProvider();
            provider = dashBoardProvider.GetServiceProvider(rootProvider);

            Assert.IsNotNull(provider.GetService<DataDictionaryPage>());
            Assert.IsNotNull(provider.GetService<IConnectorRepo>());
        }

        [TestMethod]
        public void SchemaPageRegisteredDependencies()
        {
            var dashBoardProvider = new DashboardServiceProvider();
            provider = dashBoardProvider.GetServiceProvider(rootProvider);

            Assert.IsNotNull(provider.GetService<SchemaPage>());
            Assert.IsNotNull(provider.GetService<IDataDictionaryRepo>());
        }

        [TestMethod]
        public void ObjectDetailsPageRegisteredDependencies()
        {
            var dashBoardProvider = new DashboardServiceProvider();
            provider = dashBoardProvider.GetServiceProvider(rootProvider);

            Assert.IsNotNull(provider.GetService<ObjectDetailsPage>());
            Assert.IsNotNull(provider.GetService<IDataDictionaryRepo>());
        }   
    }
}
