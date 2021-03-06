﻿using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard.Extensions;
using AutoPocoIO.Dashboard.Pages;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;

namespace AutoPocoIO.Dashboard
{
    /// <summary>
    /// Define routes with middleware dispatchers.
    /// </summary>
    public class DashboardRoutes
    {
        /// <summary>
        /// Routes for middleware dashboard.
        /// </summary>
        public RouteCollection Routes { get; }

        /// <summary>
        /// Initialize Routes.
        /// </summary>
        public DashboardRoutes()
        {
            Routes = new RouteCollection();
            Routes.Add("/", HttpMethodType.GET, new RazorPageDispatcher<DashboardPage>((p, m) => p.DailyStats()));
            Routes.Add("/Weekly", HttpMethodType.GET, new RazorPageDispatcher<DashboardPage>((p, m) => p.WeeklyStats()));

            //Connector
            Routes.Add("/Connectors", HttpMethodType.GET, new RazorPageDispatcher<ConnectorsPage>((p, m) => p.ListConnectors()));
            Routes.Add("/Connectors/Connector/New", HttpMethodType.GET, new RazorPageDispatcher<ConnectorForm>((p, m) => p.NewConnector()));
            Routes.Add("/Connectors/Delete/(?<id>.+)", HttpMethodType.POST, new CommandDispatcher<ConnectorsPage>((p, m) => p.Delete(m.ToGuid("id"))));
            Routes.Add("/Connectors/Connector/(?<id>.+)", HttpMethodType.GET, new RazorPageDispatcher<ConnectorForm>((p, m) => p.GetById(m.ToGuid("id"))));
            Routes.Add("/Connectors/Connector/(?<id>.+)", HttpMethodType.POST, new FormDispatcher<ConnectorForm>());

            //DataDictionary
            Routes.Add("/DataDictionary", HttpMethodType.GET, new RazorPageDispatcher<DataDictionaryPage>((p, m) => p.ListConnectors()));
            Routes.Add("/DataDictionary/Schema/(?<id>.+)", HttpMethodType.GET, new RazorPageDispatcher<SchemaPage>((p, m) => p.ListDbObjects(m.ToGuid("id"))));
            Routes.Add("/DataDictionary/Table/(?<id>.+)/(?<name>.+)", HttpMethodType.GET, new RazorPageDispatcher<ObjectDetailsPage>((p, m) => p.ListTableDetails(m.ToGuid("id"), m.GetString("name"))));

            //Request History
            Routes.Add("/Requests", HttpMethodType.GET, new RazorPageDispatcher<RequestHistoryPage>((p, m) => p.ListRequests()));
        }
    }
}