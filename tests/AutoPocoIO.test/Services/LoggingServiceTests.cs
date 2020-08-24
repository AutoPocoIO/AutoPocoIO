using AutoPocoIO.Constants;
using AutoPocoIO.Context;
using AutoPocoIO.Services;
using AutoPocoIO.test.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace AutoPocoIO.test.Services
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class LoggingServiceTests : DbAccessUnitTestBase
    {
        private LoggingService loggingService;
        private IServiceScope scope;

        [TestInitialize]
        public void Init()
        {
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(c => c.ServiceProvider).Returns(serviceProvider);
            scope = scopeMock.Object;

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(c => c.CreateScope()).Returns(scope);

            TimeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));
            loggingService = new LoggingService(TimeProvider.Object, scopeFactory.Object, new LoggingServiceOptions());


        }

        [TestMethod]
        public void AddTableRead()
        {
            loggingService.AddTableToLogger("conn1", "tbl1", HttpMethodType.GET);
            loggingService.LogAll().Wait();


            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.AreEqual(1, db.RequestLogs.Count());
                Assert.AreEqual(1, db.ResponseLogs.Count());

                //Check details
                Assert.AreEqual("conn1", db.RequestLogs.First().Connector);
                Assert.AreEqual("GET", db.RequestLogs.First().RequestType);
                Assert.AreEqual(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                //Check linked
                Assert.AreEqual(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.AreEqual(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }


        [TestMethod]
        public void ViewRead()
        {
            loggingService.AddViewToLogger("conn1", "vw1");
            loggingService.LogAll().Wait();

            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.AreEqual(1, db.RequestLogs.Count());
                Assert.AreEqual(1, db.ResponseLogs.Count());

                //Check details
                Assert.AreEqual("conn1", db.RequestLogs.First().Connector);
                Assert.AreEqual("GET", db.RequestLogs.First().RequestType);
                Assert.AreEqual(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                Assert.AreEqual(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.AreEqual(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }

        [TestMethod]
        public void LogExecuteProc()
        {
            loggingService.AddSprocToLogger("conn1", "proc1", HttpMethodType.POST);
            loggingService.LogAll().Wait();

            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.AreEqual(1, db.RequestLogs.Count());
                Assert.AreEqual(1, db.ResponseLogs.Count());

                //Check details
                Assert.AreEqual("conn1", db.RequestLogs.First().Connector);
                Assert.AreEqual("POST", db.RequestLogs.First().RequestType);
                Assert.AreEqual(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                Assert.AreEqual(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.AreEqual(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }

        [TestMethod]
        public void SchemaDefinition()
        {
            loggingService.AddSchemaToLogger("conn1");
            loggingService.LogAll().Wait();

            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.AreEqual(1, db.RequestLogs.Count());
                Assert.AreEqual(1, db.ResponseLogs.Count());

                //Check details
                Assert.AreEqual("conn1", db.RequestLogs.First().Connector);
                Assert.AreEqual("GET", db.RequestLogs.First().RequestType);
                Assert.AreEqual(new DateTime(2020, 1, 1), db.RequestLogs.First().DateTimeUtc);

                Assert.AreEqual(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.AreEqual(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }

        [TestMethod]
        public void LogMultipleApiOperationsPerRequest()
        {
            loggingService.AddViewToLogger("conn1", "vw1");
            loggingService.AddTableToLogger("conn1", "tbl1", HttpMethodType.DELETE);
            loggingService.LogAll().Wait();


            using (var db = new LogDbContext(LogDbOptions))
            {
                Assert.AreEqual(2, db.RequestLogs.Count());
                Assert.AreEqual(2, db.ResponseLogs.Count());

                //Check details
                Assert.AreEqual("conn1", db.RequestLogs.First().Connector);
                Assert.AreEqual("GET", db.RequestLogs.First().RequestType);

                Assert.AreEqual(db.RequestLogs.First().RequestGuid, db.ResponseLogs.First().RequestGuid);
                Assert.AreEqual(db.RequestLogs.First().RequestId, db.ResponseLogs.First().ResponseId);
            }
        }
    }
}
