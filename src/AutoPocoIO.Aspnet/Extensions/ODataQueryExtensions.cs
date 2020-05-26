using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AutoPocoIO.Extensions
{
    internal static class ODataQueryExtensions
    {
        public static ODataQueryOptions CreateOptionsFromQueryString(this ODataQueryContext context, IDictionary<string, string> queryString)
        {
            //Create request
            var url = "http://autoPocoInternalOdata.com/site?";
            foreach (var param in queryString)
            {
                url += param.Key + "=" + param.Value + "&";
            }
            url = url.Trim('?').Trim('&');

            Uri uri = new Uri(url);
            var config = new System.Web.Http.HttpConfiguration();
            config.EnableDependencyInjection();
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(1000);

            var request = new HttpRequestMessage
            {
                RequestUri = uri
            };
            request.SetConfiguration(config);

            return new ODataQueryOptions(context, request);
        }

        public static ODataConventionModelBuilder ConfigureConventionBuilder()
        {
            var config = new System.Web.Http.HttpConfiguration();
            config.EnableDependencyInjection();
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(1000);

            var modelBuilder = new ODataConventionModelBuilder(config, true);
            return modelBuilder;
        }
    }
}
