using AutoPocoIO.Constants;
using AutoPocoIO.Context;
using AutoPocoIO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.Dashboard.Repos
{
    internal class DashboardRepo : IDashboardRepo
    {
        private readonly LogDbContext _db;
        private readonly ITimeProvider _timeProvider;

        private static readonly string[] requestTypes = new[]
        {
            nameof(HttpMethodType.GET),
            nameof(HttpMethodType.POST),
            nameof(HttpMethodType.PUT),
            nameof(HttpMethodType.DELETE)
        };
        public DashboardRepo(LogDbContext db, ITimeProvider timeProvider)
        {
            _db = db;
            _timeProvider = timeProvider;
        }


        public virtual int TotalRequests(int daysAgo)
        {
            DateTime searchDate = _timeProvider.LocalToday.AddDays(Math.Abs(daysAgo) * -1);

            return _db.RequestLogs.Where(c => requestTypes.Contains(c.RequestType))
                                  .Where(c => c.DateTimeUtc >= searchDate)
                                .Count();
        }

        public virtual int SuccessFulRequests(int daysAgo)
        {
            return GetCountByStatus(daysAgo, c => c == "HTTP 200 : OK");
        }

        public virtual int FailedRequests(int daysAgo)
        {
            return GetCountByStatus(daysAgo, c => c != "HTTP 200 : OK");
        }

        public virtual int UnauthorizedRequest(int daysAgo)
        {
            return GetCountByStatus(daysAgo, c => c == "HTTP 401 : Unauthorized");
        }

        public virtual int TotalRequestsTime(int daysAgo)
        {
            return GetResponseTimeByStatus(daysAgo, c => true);
        }

        public virtual int SuccessFulRequestsTime(int daysAgo)
        {
            return GetResponseTimeByStatus(daysAgo, c => c == "HTTP 200 : OK");
        }

        public virtual int FailedRequestsTime(int daysAgo)
        {
            return GetResponseTimeByStatus(daysAgo, c => c != "HTTP 200 : OK");
        }

        public virtual int UnauthorizedRequestTime(int daysAgo)
        {
            return GetResponseTimeByStatus(daysAgo, c => c == "HTTP 401 : Unauthorized");
        }

        public virtual Tuple<string[], int[], int[]> HourlyRequest()
        {
            DateTime searchDate = _timeProvider.UtcNow.AddDays(-1);
            var offset = TimeZoneInfo.Local.GetUtcOffset(_timeProvider.Now);

            var requests = _db.RequestLogs.GroupJoin(_db.ResponseLogs, c => new { c.RequestId, c.RequestGuid },
                                                   c => new { RequestId = c.ResponseId, c.RequestGuid },
                                                   (req, resp) => new { RequestTime = req.DateTimeUtc, IsSuccess = resp.FirstOrDefault().Status == "HTTP 200 : OK" })
                                          .Where(c => c.RequestTime >= searchDate);

            var groups = requests.GroupBy(c => new
            {
                Time = new DateTime(c.RequestTime.Value.Year,
                c.RequestTime.Value.Month,
                c.RequestTime.Value.Day,
                //Round down to 2 hour
                c.RequestTime.Value.Hour - (c.RequestTime.Value.Hour % 2),
                0, 0),
                Result = c.IsSuccess
            }).Select(c => new
            {
                Time = c.Key.Time + offset,
                c.Key.Result,
                Count = c.Count()
            }
            ).ToList();


            var keyTime = searchDate + offset;
            keyTime = new DateTime(keyTime.Year,
                keyTime.Month,
                keyTime.Day,
                //Round down to 2 hour
                keyTime.Hour - (keyTime.Hour % 2),
                0, 0);

            var keys = new List<string>();
            var success = new List<int>();
            var fail = new List<int>();
            for (int i = 0; i < 13; i++)
            {
                keyTime = keyTime.AddHours(2);
                keys.Add(keyTime.ToShortTimeString());

                //Get values
                var vals = groups.Where(c => c.Time == keyTime);

                var successVal = vals.FirstOrDefault(c => c.Result);
                var failVal = vals.FirstOrDefault(c => !c.Result);

                if (successVal == null)
                    success.Add(0);
                else
                    success.Add(successVal.Count);

                if (failVal == null)
                    fail.Add(0);
                else
                    fail.Add(failVal.Count);

            }


            return new Tuple<string[], int[], int[]>
            (
                keys.ToArray(),
                success.ToArray(),
                fail.ToArray()
            );
        }

        public virtual Tuple<string[], int[], int[]> WeeklyRequest()
        {
            DateTime searchDate = _timeProvider.UtcNow.AddDays(-6);
            var offset = TimeZoneInfo.Local.GetUtcOffset(_timeProvider.Now);

            var requests = _db.RequestLogs.GroupJoin(_db.ResponseLogs, c => new { c.RequestId, c.RequestGuid },
                                                   c => new { RequestId = c.ResponseId, c.RequestGuid },
                                                   (req, resp) => new { RequestTime = req.DateTimeUtc, IsSuccess = resp.FirstOrDefault().Status == "HTTP 200 : OK" })
                                          .Where(c => c.RequestTime >= searchDate);

            var groups = requests.GroupBy(c => new
            {
                Time = new DateTime(c.RequestTime.Value.Year,
                c.RequestTime.Value.Month,
                (c.RequestTime.Value + offset).Day),
                Result = c.IsSuccess
            }).Select(c => new
            {
                c.Key.Time,
                c.Key.Result,
                Count = c.Count()
            }
            ).ToList();


            var keyTime = searchDate + offset;
            keyTime = new DateTime(keyTime.Year,
                keyTime.Month,
                keyTime.Day);

            var keys = new List<string>();
            var success = new List<int>();
            var fail = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if(i != 0)
                    keyTime = keyTime.AddDays(1);
                keys.Add(keyTime.DayOfWeek.ToString());

                //Get values
                var vals = groups.Where(c => c.Time == keyTime);

                var successVal = vals.FirstOrDefault(c => c.Result);
                var failVal = vals.FirstOrDefault(c => !c.Result);

                if (successVal == null)
                    success.Add(0);
                else
                    success.Add(successVal.Count);

                if (failVal == null)
                    fail.Add(0);
                else
                    fail.Add(failVal.Count);

            }


            return new Tuple<string[], int[], int[]>
            (
                keys.ToArray(),
                success.ToArray(),
                fail.ToArray()
            );
        }

        private int GetCountByStatus(int daysAgo, Func<string, bool> status)
        {
            DateTime searchDate = _timeProvider.LocalToday.AddDays(Math.Abs(daysAgo) * -1);

            var requests = _db.RequestLogs.GroupJoin(_db.ResponseLogs, c => new { c.RequestId, c.RequestGuid },
                                                    c => new { RequestId = c.ResponseId, c.RequestGuid },
                                                    (req, resp) => new { req.DateTimeUtc, resp.FirstOrDefault().Status });

            return requests.Where(c => c.DateTimeUtc >= searchDate && status.Invoke(c.Status))
                        .Count();
        }

        private int GetResponseTimeByStatus(int daysAgo, Func<string, bool> status)
        {
            DateTime searchDate = _timeProvider.LocalToday.AddDays(Math.Abs(daysAgo) * -1);

            var requests = _db.RequestLogs.GroupJoin(_db.ResponseLogs, c => new { c.RequestId, c.RequestGuid },
                                                    c => new { RequestId = c.ResponseId, c.RequestGuid },
                                                    (req, resp) => new { RequestTime = req.DateTimeUtc, resp.FirstOrDefault().Status, ResponseTime = resp.FirstOrDefault().DateTimeUtc })
                                        .Where(c => c.RequestTime >= searchDate && status.Invoke(c.Status))
                                        .Where(c => c.RequestTime != null && c.ResponseTime != null);
            if (requests.Any())
                return (int)requests.Average(c => (c.ResponseTime - c.RequestTime).Value.TotalMilliseconds);
            else
                return 0;
        }
    }
}