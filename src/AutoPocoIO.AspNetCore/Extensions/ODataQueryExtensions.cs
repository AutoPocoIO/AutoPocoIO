using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Builder;
#if EF22
using Microsoft.AspNetCore.Builder.Internal;
#endif
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.Extensions
{
    internal static class ODataQueryExtensions
    {
        public static ODataQueryOptions CreateOptionsFromQueryString(this ODataQueryContext context, IDictionary<string, string> queryString)
        {
            var collection = new ServiceCollection();
            collection.AddOData();
            var provider = collection.BuildServiceProvider();

            var appBuilder = new ApplicationBuilder(provider);

            var routeBuilder = new Microsoft.AspNetCore.Routing.RouteBuilder(appBuilder);
            routeBuilder.EnableDependencyInjection();
            routeBuilder.Count().Filter().OrderBy().Expand().Select().MaxTop(1000);


            //Create request
            var url = "http://autoPocoInternalOdata.com/site?";
            foreach (var param in queryString)
            {
                url += param.Key + "=" + param.Value + "&";
            }
            url = url.Trim('?').Trim('&');

            var uri = new Uri(url);
            var httpContext = new DefaultHttpContext
            {
                RequestServices = provider
            };

            httpContext.Request.Method = "GET";
            httpContext.Request.Host = new HostString(uri.Host);
            httpContext.Request.Scheme = uri.Scheme;
            httpContext.Request.Path = uri.LocalPath;
            httpContext.Request.QueryString = new QueryString(uri.Query);

            return new ODataQueryOptions(context, httpContext.Request);
        }

        public static ODataConventionModelBuilder ConfigureConventionBuilder()
        {
            var collection = new ServiceCollection();
            collection.AddOData();
            var provider = collection.BuildServiceProvider();

            var modelBuilder = new ODataConventionModelBuilder(provider, true);
            return modelBuilder;
        }
    }
}
