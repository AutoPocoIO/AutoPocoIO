using AutoPocoIO.Context;
using AutoPocoIO.Dashboard;
using AutoPocoIO.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace AutoPocoIO.test.Dashboard.ServiceTests
{
    public abstract class DashboardServiceTests
    {
        protected IServiceProvider provider;
        protected IServiceProvider rootProvider;

        [TestInitialize]
        public void Init()
        {
            //Clear static provider field to reset test state
            var dashBoardProvider = new DashboardServiceProvider();
            Type type = typeof(DashboardServiceProvider);
            FieldInfo info = type.GetField("_provider", BindingFlags.NonPublic | BindingFlags.Static);
            info.SetValue(dashBoardProvider, null);

            ServiceCollection rootCollection = new ServiceCollection();
            rootCollection.AddSingleton<IContextEntityConfiguration>(new ContextEntityConfiguration());
            rootCollection.AddSingleton<DbContextOptions<AppDbContext>>();
            rootCollection.AddSingleton<DbContextOptions<LogDbContext>>();

            rootProvider = rootCollection.BuildServiceProvider();
        }
    }
}
