using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace AutoPocoIO.test.Dashboard.Repos
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class RequestHistoryRepoTests
    {
        private DbContextOptions<LogDbContext> dbOptions;
        private IRequestHistoryRepo repo;
        private DateTime today;
        private Guid guid;
        [TestInitialize]
        public void Init()
        {
            today = new DateTime(2011, 1, 2);
            dbOptions = new DbContextOptionsBuilder<LogDbContext>()
                .UseInMemoryDatabase(databaseName: "db" + Guid.NewGuid().ToString())
                .Options;

            var timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(c => c.UtcOffset).Returns(TimeSpan.FromHours(-4));

            var db = new LogDbContext(dbOptions);
            repo = new RequestHistoryRepo(db, timeProvider.Object);

            guid = Guid.NewGuid();
        }

        [TestMethod]
        public void GetRecentRequests()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), Status = "HTTP 302 : Found" }
                );

                db1.SaveChanges();
            }

            var results = repo.ListRequest(20);
            Assert.AreEqual(2, results.Count());
            Assert.AreEqual("GET", results.First().RequestType);
            Assert.AreEqual(new DateTime(2011, 1, 1, 21, 0, 0), results.First().DateTimeUtc);
        }

        [TestMethod]
        public void GetNullDate()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, RequestType = "GET" }
                    );


                db1.SaveChanges();
            }

            var results = repo.ListRequest(20);
            Assert.IsNull(results.First().DateTimeUtc);
        }

        [TestMethod]
        public void TakeOnlyRecordLimit()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, RequestType = "GET" },
                     new RequestLog { RequestId = 2, RequestGuid = guid, RequestType = "GET" }
                    );


                db1.SaveChanges();
            }

            var results = repo.ListRequest(1);
            Assert.AreEqual(1, results.Count());
        }
    }
}
