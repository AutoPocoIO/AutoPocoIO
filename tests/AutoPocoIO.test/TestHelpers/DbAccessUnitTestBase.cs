using AutoPocoIO.Context;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using System;

namespace AutoPocoIO.test.TestHelpers
{
    public abstract class DbAccessUnitTestBase
    {

        internal DbContextOptions<LogDbContext> LogDbOptions { get; set; }
        internal DbContextOptions<AppDbContext> AppDbOptions { get; set; }
        protected IServiceProvider serviceProvider;
        internal Mock<ITimeProvider> TimeProvider { get; set; }
        protected IServiceScopeFactory serviceScopeFactory;

        public DbAccessUnitTestBase()
        {
            LogDbOptions = new DbContextOptionsBuilder<LogDbContext>()
               .UseInMemoryDatabase(databaseName: "logDb" + Guid.NewGuid().ToString())
               .Options;

            AppDbOptions = new DbContextOptionsBuilder<AppDbContext>()
               .UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString())
               .Options;

            //Set up DI
            var logDb = new LogDbContext(LogDbOptions);
            var appDb = new AppDbContext(AppDbOptions);

            TimeProvider = new Mock<ITimeProvider>();

            ServiceCollection services = new ServiceCollection();
            services.AddScoped(c => logDb)
                .AddScoped(c => appDb)
                .AddSingleton(c => TimeProvider.Object);


            serviceProvider = services.BuildServiceProvider();


            var _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceProvider.CreateScope());

            serviceScopeFactory = _serviceScopeFactory.Object;
        }
    }
}
