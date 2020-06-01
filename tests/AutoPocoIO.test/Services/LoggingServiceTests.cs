using AutoPocoIO.Constants;
using AutoPocoIO.Context;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace AutoPocoIO.test.Services
{
    [Trait("Category", TestCategories.Unit)]
    public class LoggingServiceTests
    {
        private readonly LoggingService loggingService;
        private readonly DbContextOptions<LogDbContext> LogDbOptions;
        private readonly IServiceScope scope;

        public  LoggingServiceTests()
        {
            LogDbOptions = new DbContextOptionsBuilder<LogDbContext>()
               .UseInMemoryDatabase(databaseName: "logDb" + Guid.NewGuid().ToString())
               .Options;

            var timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));
            loggingService = new LoggingService(timeProvider.Object);

            ServiceCollection services = new ServiceCollection();
            services.AddSingleton(c => new LogDbContext(LogDbOptions))
                    .AddSingleton(c => timeProvider.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(c => c.ServiceProvider).Returns(services.BuildServiceProvider());
            scope = scopeMock.Object;
        }

        [FactWithName]
        public void AddTableRead()
        {
            loggingService.AddTableToLogger("conn1", "tbl1", HttpMethodType.GET);
            loggingService.LogAll(scope);


            using (var db = new LogDbContext(LogDbOptions))
            {
                Xunit.Assert.Equal(1, db.RequestLogs.Count());
                Assert.Equal(1, db.ResponseLogs.Count());

                //Check details
                Assert.Equal("conn1", db.RequestLogs.First().Connector);
                Assert.Equal("GET", db.RequestLogs.First().RequestType);
                Assert.Equal(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                //Check linked
                Assert.Equal(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.Equal(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }

      
        [FactWithName]
        public void ViewRead()
        {
            loggingService.AddViewToLogger("conn1", "vw1");
            loggingService.LogAll(scope);

            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.Equal(1, db.RequestLogs.Count());
                Assert.Equal(1, db.ResponseLogs.Count());

                //Check details
                Assert.Equal("conn1", db.RequestLogs.First().Connector);
                Assert.Equal("GET", db.RequestLogs.First().RequestType);
                Assert.Equal(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                Assert.Equal(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.Equal(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }

        [FactWithName]
        public void LogExecuteProc()
        {
            loggingService.AddSprocToLogger("conn1", "proc1", HttpMethodType.POST);
            loggingService.LogAll(scope);

            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.Equal(1, db.RequestLogs.Count());
                Assert.Equal(1, db.ResponseLogs.Count());

                //Check details
                Assert.Equal("conn1", db.RequestLogs.First().Connector);
                Assert.Equal("POST", db.RequestLogs.First().RequestType);
                Assert.Equal(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                Assert.Equal(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.Equal(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }

        [FactWithName]
        public void SchemaDefinition()
        {
            loggingService.AddSchemaToLogger("conn1");
            loggingService.LogAll(scope);

            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.Equal(1, db.RequestLogs.Count());
                Assert.Equal(1, db.ResponseLogs.Count());

                //Check details
                Assert.Equal("conn1", db.RequestLogs.First().Connector);
                Assert.Equal("GET", db.RequestLogs.First().RequestType);
                Assert.Equal(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                Assert.Equal(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.Equal(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }

        [FactWithName]
        public void LogMultipleApiOperationsPerRequest()
        {
            loggingService.AddViewToLogger("conn1", "vw1");
            loggingService.AddTableToLogger("conn1", "tbl1", HttpMethodType.DELETE);
            loggingService.LogAll(scope);


            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.Equal(2, db.RequestLogs.Count());
                Assert.Equal(2, db.ResponseLogs.Count());

                //Check details
                Assert.Equal("conn1", db.RequestLogs.First().Connector);
                Assert.Equal("GET", db.RequestLogs.First().RequestType);

                Assert.Equal(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.Equal(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }
    }
}
