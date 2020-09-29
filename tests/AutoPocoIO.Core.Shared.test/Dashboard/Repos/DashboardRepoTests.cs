using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace AutoPocoIO.test.Dashboard.Repos
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DashboardRepoTests
    {
        private DbContextOptions<LogDbContext> dbOptions;
        private IDashboardRepo repo;
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
            timeProvider.Setup(c => c.UtcNow).Returns(today.AddHours(9)); //0900 UTC
            timeProvider.Setup(c => c.Now).Returns(today.AddHours(7)); //0500 local
            timeProvider.Setup(c => c.LocalToday).Returns(today.AddHours(-4));  //Local time is previous day from 0000 UTC
            timeProvider.Setup(c => c.UtcOffset).Returns(TimeSpan.FromHours(-4));

            var db = new LogDbContext(dbOptions);
            repo = new DashboardRepo(db, timeProvider.Object);

            guid = Guid.NewGuid();
        }

        [TestMethod]
        public void GetRequestsForToday()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, DateTimeUtc = today.AddHours(1), RequestType = "GET" }
                    );

                db1.SaveChanges();
            }

            int count = repo.TotalRequests(0);

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void GetRequestsForLocalToday()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, DateTimeUtc = today.AddHours(-3), RequestType = "GET" }, //Yesterday UTC but today local
                    new RequestLog { RequestId = 3, DateTimeUtc = today.AddDays(-1), RequestType = "GET" }//Yesterday UTC and  yesterdat local (exclude)
                    );

                db1.SaveChanges();
            }

            int count = repo.TotalRequests(0);

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void GetRequestsOnlyHttpOps()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, DateTimeUtc = today, RequestType = "GET" },
                     new RequestLog { RequestId = 2, DateTimeUtc = today, RequestType = "POST" },
                      new RequestLog { RequestId = 3, DateTimeUtc = today, RequestType = "PUT" },
                       new RequestLog { RequestId = 4, DateTimeUtc = today, RequestType = "DELETE" },
                        new RequestLog { RequestId = 5, DateTimeUtc = today, RequestType = "Other" }
                    );

                db1.SaveChanges();
            }

            int count = repo.TotalRequests(0);

            Assert.AreEqual(4, count);
        }


        [TestMethod]
        public void GetRequestsForLocal2Days()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, DateTimeUtc = today.AddHours(-3), RequestType = "GET" }, //Yesterday UTC but today local
                    new RequestLog { RequestId = 3, DateTimeUtc = today.AddDays(-1), RequestType = "GET" }//Include yesterday
                    );

                db1.SaveChanges();
            }

            int count = repo.TotalRequests(1);

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void GetRequestsForLocal2DaysNegativePassed()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, DateTimeUtc = today.AddHours(-3), RequestType = "GET" }, //Yesterday UTC but today local
                    new RequestLog { RequestId = 3, DateTimeUtc = today.AddDays(-1), RequestType = "GET" }//Include yesterday
                    );

                db1.SaveChanges();
            }

            int count = repo.TotalRequests(-1);

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void TotalRequestsTimeNoValuesReturns0()
        {
            int responseTime = repo.TotalRequestsTime(0);
            Assert.AreEqual(0, responseTime);
        }

        [TestMethod]
        public void TotalRequestsTime()
        {

            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1).AddSeconds(2), Status = "HTTP 302 : Found" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2).AddSeconds(7), Status = "Other" }
                );

                db1.SaveChanges();
            }

            int count = repo.TotalRequestsTime(0);

            Assert.AreEqual(3000, count);
        }

        [TestMethod]
        public void CountSuccessfullRequests()
        {

            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), Status = "HTTP 302 : Found" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), Status = "Other" }
                );

                db1.SaveChanges();
            }

            int count = repo.SuccessFullRequests(0);

            Assert.AreEqual(2, count);
        }


        [TestMethod]
        public void SuccessfullRequestsTime()
        {

            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1).AddSeconds(2), Status = "HTTP 302 : Found" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2).AddSeconds(7), Status = "Other" }
                );

                db1.SaveChanges();
            }

            int count = repo.SuccessFullRequestsTime(0);

            Assert.AreEqual(1000, count);
        }

        [TestMethod]
        public void CountFailedRequests()
        {

            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), Status = "HTTP 302 : Found" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), Status = "Other" }
                );

                db1.SaveChanges();
            }

            int count = repo.FailedRequests(0);

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void FailedRequestsTime()
        {

            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1).AddSeconds(2), Status = "Other" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2).AddSeconds(7), Status = "Other" }
                );

                db1.SaveChanges();
            }

            int count = repo.FailedRequestsTime(0);

            Assert.AreEqual(4500, count);
        }


        [TestMethod]
        public void UnauthorizedRequest()
        {

            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), Status = "HTTP 401 : Unauthorized" }
                );

                db1.SaveChanges();
            }

            int count = repo.UnauthorizedRequest(0);

            Assert.AreEqual(1, count);
        }


        [TestMethod]
        public void UnauthorizedRequestsTime()
        {

            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1).AddSeconds(2), Status = "Other" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2).AddSeconds(7), Status = "HTTP 401 : Unauthorized" }
                );

                db1.SaveChanges();
            }

            int count = repo.UnauthorizedRequestTime(0);

            Assert.AreEqual(7000, count);
        }

        [TestMethod]
        public void GetByTodaysStatusBy2Hour()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddHours(1), Status = "HTTP 302 : Found" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddHours(2), Status = "Other" }
                );

                db1.SaveChanges();
            }

            var results = repo.HourlyRequest();

            //All Arrays are equal length
            Assert.AreEqual(13, results.Item1.Length);
            Assert.AreEqual(13, results.Item2.Length);
            Assert.AreEqual(13, results.Item3.Length);

            //Check X Axis values
            Assert.AreEqual("6:00 AM", results.Item1[0]);
            Assert.AreEqual("6:00 PM", results.Item1[6]);
            Assert.AreEqual("6:00 AM", results.Item1[12]);

            //Check values
            Assert.AreEqual(2, results.Item2[7]); //success
            Assert.AreEqual(1, results.Item3[8]); //fails
        }

        [TestMethod]
        public void GetByTodaysStatusByDays()
        {
            using (var db1 = new LogDbContext(dbOptions))
            {
                db1.RequestLogs.AddRange(
                    new RequestLog { RequestId = 1, RequestGuid = guid, DateTimeUtc = today, RequestType = "GET" },
                    new RequestLog { RequestId = 2, RequestGuid = guid, DateTimeUtc = today.AddDays(-1), RequestType = "GET" },
                    new RequestLog { RequestId = 3, RequestGuid = guid, DateTimeUtc = today.AddDays(-2), RequestType = "GET" }
                    );

                db1.ResponseLogs.AddRange(
                    new ResponseLog { ResponseId = 1, RequestGuid = guid, DateTimeUtc = today, Status = "HTTP 200 : OK" },
                    new ResponseLog { ResponseId = 2, RequestGuid = guid, DateTimeUtc = today.AddDays(-1), Status = "HTTP 302 : Found" },
                    new ResponseLog { ResponseId = 3, RequestGuid = guid, DateTimeUtc = today.AddDays(-2), Status = "Other" }
                );

                db1.SaveChanges();
            }

            var results = repo.WeeklyRequest();

            //All Arrays are equal length
            Assert.AreEqual(7, results.Item1.Length);
            Assert.AreEqual(7, results.Item2.Length);
            Assert.AreEqual(7, results.Item3.Length);

            //Check X Axis values
            Assert.AreEqual("Monday", results.Item1[0]);
            Assert.AreEqual("Thursday", results.Item1[3]);
            Assert.AreEqual("Sunday", results.Item1[6]);

            //Check values
            Assert.AreEqual(1, results.Item2[4]); //success
            Assert.AreEqual(1, results.Item2[5]); //success
            Assert.AreEqual(1, results.Item3[3]); //fails
        }
    }
}
