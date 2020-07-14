using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class DashboardPage
    {
        private readonly IDashboardRepo _repo;
        public DashboardPage(IDashboardRepo repo, ILayoutPage layoutPage)
            :base(layoutPage, "Overview - AutoPoco")
        {
            _repo = repo;
        }

        public virtual void DailyStats()
        {
            GetCounts(0);
            ViewBag["IsDaily"] = "active";
            ViewBag["IsWeekly"] = "";

            var graph = _repo.HourlyRequest();
            ViewBag["GraphLabels"] = graph.Item1;
            ViewBag["SuccessfulGraph"] = graph.Item2;
            ViewBag["FailGraph"] = graph.Item3;
        }

        public virtual void WeeklyStats()
        {
            GetCounts(-6);
            ViewBag["IsWeekly"] = "active";
            ViewBag["IsDaily"] = "";

            var graph = _repo.WeeklyRequest();
            ViewBag["GraphLabels"] = graph.Item1;
            ViewBag["SuccessfulGraph"] = graph.Item2;
            ViewBag["FailGraph"] = graph.Item3;
        }

        private void GetCounts(int addDays)
        {
            ViewBag["TotalCount"] = _repo.TotalRequests(addDays);
            ViewBag["SuccessfulCount"] = _repo.SuccessFullRequests(addDays);
            ViewBag["FailCount"] = _repo.FailedRequests(addDays);
            ViewBag["UnauthorizedCount"] = _repo.UnauthorizedRequest(addDays);

            ViewBag["TotalTime"] = _repo.TotalRequestsTime(addDays);
            ViewBag["SuccessfulTime"] = _repo.SuccessFullRequestsTime(addDays);
            ViewBag["FailCountTime"] = _repo.FailedRequestsTime(addDays);
            ViewBag["UnauthorizedTime"] = _repo.UnauthorizedRequestTime(addDays);
        }

       
    }
}
