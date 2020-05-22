using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard.Extensions;
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
            Routes.Add("/", HttpMethodType.GET, new RazorPageDispatcher<DashboardPage>((p, m) => p.DailyStats()));
            Routes.Add("/Weekly", HttpMethodType.GET, new RazorPageDispatcher<DashboardPage>((p, m) => p.WeeklyStats()));

            //Connector
            Routes.Add("/Connectors", HttpMethodType.GET, new RazorPageDispatcher<ConnectorsPage>((p, m) => p.ListConnectors()));
            Routes.Add("/Connectors/Connector/New", HttpMethodType.GET, new RazorPageDispatcher<ConnectorForm>((p, m) => p.NewConnector()));
            Routes.Add("/Connectors/Connector/(?<id>.+)", HttpMethodType.GET, new RazorPageDispatcher<ConnectorForm>((p, m) => p.GetById(m.ToInt("id"))));
            Routes.Add("/Connectors/Connector/(?<id>.+)", HttpMethodType.POST, new FormDispatcher<ConnectorForm>());
        }
    }
}