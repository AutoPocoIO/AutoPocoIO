using AutoPocoIO.Context;
using AutoPocoIO.Dashboard;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Factories;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.Dashboard.ServiceTests
{
    [TestClass]
    [TestCategory(TestCategories.Integration)]
    public class GeneralDashboardServiceTests : DashboardServiceTests
    {
        [TestMethod]
        public void AllGeneralRegistrationsWork()
        {
            var dashBoardProvider = new DashboardServiceProvider();
            provider = dashBoardProvider.GetServiceProvider(rootProvider);


            Assert.IsNotNull(provider.GetService<IResourceFactory>());
            Assert.IsNotNull(provider.GetService<IAppAdminService>());

            Assert.IsNotNull(provider.GetService<IConnectionStringFactory>());
            Assert.IsNotNull(provider.GetService<AppDbContext>());
            Assert.IsNotNull(provider.GetService<LogDbContext>());

            Assert.IsNotNull(provider.GetService<IDashboardRepo>());
        }
    }
}
