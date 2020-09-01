using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AutoPocoIO.AspNet.test.Owin
{
    public partial class DashboardMiddlewareTests
    {
        private class TestRoutes : DashboardRoutes
        {
            public TestRoutes()
            {
                Routes.Add("/forGet", HttpMethodType.GET, new RazorPageDispatcher(new TestPage()));
            }
        }

        private class TestPage : RazorPage
        {
            public override void Execute()
            {
                WriteLiteral("testPageInfo");
                WriteLiteral(Context.QueryStrings.FirstOrDefault().Key);
                WriteLiteral(Context.QueryStrings.FirstOrDefault().Value);
            }
        }

        private class ReplaceRoutes : IReplaceServices<DashboardServiceProvider>
        {
            public IServiceCollection ReplaceInternalServices(IServiceProvider rootProvider, IServiceCollection services)
            {
                services.AddSingleton<DashboardRoutes>(new TestRoutes());
                return services;
            }
        }
    }
}
