using AutoPocoIO.Context;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.test.Api
{

    public abstract class ApiOperationBase
    {
        protected LoggingServiceCheckApi loggingService;
        internal Mock<IResourceFactory> resourceFactoryMock;
        protected IServiceProvider serviceProvider;
        internal AppDbContext db;
        protected IQueryable<object> iqueryable;

        protected class LoggingServiceCheckApi : LoggingService
        {
            public LoggingServiceCheckApi(ITimeProvider timeProvider) : base(timeProvider)
            {
            }

            public new List<LogRequestAndResponseCommand> ApiRequests => base.ApiRequests;
        }

        [TestInitialize]
        public void Init()
        {
            var appDbOptions = new DbContextOptionsBuilder<AppDbContext>()
             .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
             .Options;

            db = new AppDbContext(appDbOptions);

            var iqueryableMock = new Mock<IQueryable<object>>();
            iqueryableMock.Setup(c => c.ElementType).Returns(typeof(IQueryableType));
            iqueryable = iqueryableMock.Object;

            resourceFactoryMock = new Mock<IResourceFactory>();

            var queryStringService = new Mock<IRequestQueryStringService>();
            queryStringService.Setup(c => c.GetQueryStrings())
                .Returns(new Dictionary<string, string>());

            var timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

            var services = new ServiceCollection();
            services.AddTransient(c => resourceFactoryMock.Object);
            services.AddTransient(c => queryStringService.Object);
            services.AddTransient(c => db);
            services.AddTransient(c => timeProvider.Object);

            serviceProvider = services.BuildServiceProvider();

            loggingService = new LoggingServiceCheckApi(timeProvider.Object);
        }
    }
}
