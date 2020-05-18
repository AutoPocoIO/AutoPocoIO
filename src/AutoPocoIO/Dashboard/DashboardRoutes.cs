using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;

namespace AutoPocoIO.Dashboard
{
    internal class DashboardRoutes
    {
        public RouteCollection Routes { get; }

        public DashboardRoutes()
        {
            Routes = new RouteCollection();
            Routes.Add("/", new RazorPageDispatcher<DashboardPage>((p, m) => p.DailyStats()));
            Routes.Add("/Weekly", new RazorPageDispatcher<DashboardPage>((p, m) => p.WeeklyStats()));
        }
    }
}