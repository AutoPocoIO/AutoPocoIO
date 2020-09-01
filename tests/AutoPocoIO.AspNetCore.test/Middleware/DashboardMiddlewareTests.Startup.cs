using AutoPocoIO.Constants;
using AutoPocoIO.Context;
using AutoPocoIO.Dashboard;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;

namespace AutoPocoIO.AspNetCore.test.Middleware
{
    public partial class DashboardMiddlewareTests
    {
        private class TestStartup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton(logger.Object);

                
                Mock<IServiceScopeFactory> scopeProvider = new Mock<IServiceScopeFactory>();
                scopeProvider.Setup(c => c.CreateScope()).Returns(scope.Object);
                services.AddSingleton(scopeProvider.Object);

                Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
                timeProvider.Setup(c => c.UtcNow).Returns(new DateTime(2020, 1, 1));

                services.AddSingleton(timeProvider.Object);
                services.AddSingleton(Mock.Of<DbContextOptions<AppDbContext>>());
                services.AddSingleton(Mock.Of<DbContextOptions<LogDbContext>>());
                services.AddSingleton<IReplaceServices<DashboardServiceProvider>>(new ReplaceRoutes());
                
            }
            public void Configure(IApplicationBuilder app)
            {
                app.UseMiddleware<AspNetCoreDashboardMiddleware>();
                app.UseMiddleware<EndOfPipeLineTestMiddleware>();
            }
        }

        private class TestRoutes : DashboardRoutes
        {
            public TestRoutes()
            {
                Routes.Add("/forGet", HttpMethodType.GET, new RazorPageDispatcher<TestPage>((p, m) => { }));
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
                services.AddTransient<TestPage>();
                return services;
            }
        }
    }
}
