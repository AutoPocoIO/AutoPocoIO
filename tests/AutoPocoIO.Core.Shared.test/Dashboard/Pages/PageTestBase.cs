using AutoPocoIO.Context;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Middleware;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.test.Dashboard.Pages
{
    public class PageTestBase
    {
        protected class LoggingServiceWtihPublicApi : LoggingService
        {
            public LoggingServiceWtihPublicApi(ITimeProvider timeProvider, IServiceScopeFactory scopeFactory) : base(timeProvider, scopeFactory, new AutoPocoServiceOptions())
            {
            }

            public IEnumerable<LogRequestAndResponseCommand> PublicRequests => base.ApiRequests;
        }

        internal DbContextOptions<AppDbContext> AppDbOptions { get; set; }
        protected LoggingServiceWtihPublicApi Logging { get; set; }
        protected ServiceCollection Services { get; set; }
        protected IMiddlewareContext Context { get; set; }

        [TestInitialize]
        public void InitBase()
        {
            AppDbOptions = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(databaseName: "db" + Guid.NewGuid().ToString())
              .Options;

            var db = new AppDbContext(AppDbOptions);

            var timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory.Setup(c => c.CreateScope());

            Logging = new LoggingServiceWtihPublicApi(timeProvider.Object, serviceScopeFactory.Object);

            SetupContext("get");
        }

        protected void SetupContext(string method)
        {
            var req = new Mock<IMiddlewareRequest>();
            req.SetupGet(c => c.Method).Returns(method);

            var mock = new Mock<IMiddlewareContext>();
            mock.SetupAllProperties();

            mock.SetupGet(c => c.Request).Returns(req.Object);
            mock.SetupGet(c => c.Response).Returns(Mock.Of<IMiddlewareResponse>());

            Services = new ServiceCollection();
            Services.AddSingleton(Mock.Of<ILayoutPage>());
            mock.Setup(c => c.InternalServiceProvider).Returns(() => Services.BuildServiceProvider());

            Context = mock.Object;
        }
    }
}
