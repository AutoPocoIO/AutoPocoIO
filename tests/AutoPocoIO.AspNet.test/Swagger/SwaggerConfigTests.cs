using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Extensions;
using AutoPocoIO.SwaggerAddons;
using Microsoft.Owin.Testing;
using Xunit;
using Newtonsoft.Json.Linq;
using Owin;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

namespace AutoPocoIO.AspNet.test.Swagger
{
    
    [Trait("Category", TestCategories.Unit)]
    public class SwaggerConfigTests
    {
        [RoutePrefix("SwaggerConfg")]
        private class SwaggerConfigController : ApiController
        {
            [UseOdataInSwagger]
            [HttpGet]
            [Route("Odata")]
            public HttpResponseMessage Odata() { throw new NotImplementedException(); }

        }
        protected HttpConfiguration Config { get; set; }
        protected SwaggerDocsHandler Handler { get; set; }

        public SwaggerConfigTests()
        {
            Config = new HttpConfiguration();
            AutoPocoConfiguration.DashboardPathPrefix = null;
        }

        protected void SetUpDefaultRoutesFor(IEnumerable<Type> controllerTypes)
        {
            foreach (var type in controllerTypes)
            {
                var controllerName = type.Name.ToLower().Replace("controller", String.Empty);
                var route = new HttpRoute(
                    String.Format("{0}/{{id}}", controllerName),
                    new HttpRouteValueDictionary(new { controller = controllerName, id = RouteParameter.Optional }));
                Config.Routes.Add(controllerName, route);
            }
        }

        protected void SetUpDefaultRouteFor<TController>()
           where TController : ApiController
        {
            SetUpDefaultRoutesFor(new[] { typeof(TController) });
        }

        protected void SetUpCustomRouteFor<TController>(string routeTemplate)
            where TController : ApiController
        {
            var controllerName = typeof(TController).Name.ToLower().Replace("controller", String.Empty);
            var route = new HttpRoute(
                routeTemplate,
                new HttpRouteValueDictionary(new { controller = controllerName, id = RouteParameter.Optional }));
            Config.Routes.Add(controllerName, route);
        }




        [FactWithName]
        public void RedirctToSwaggerPathStartsWithDashPath()
        {
            using (var server = TestServer.Create(app =>
            {
                SwaggerConfig.Register(Config, "/autoPocoPath");
                app.UseWebApi(Config);
            }))
            {
                var req = server.CreateRequest("autoPocoPath/swagger");
                var resp = req.GetAsync().Result;

                Assert.Equal(301, (int)resp.StatusCode);
                Assert.Equal("/autoPocoPath/swagger/ui/index", resp.Headers.Location.AbsolutePath);
            }
        }

        [FactWithName]
        public void VerifyDocumentTitleAutoPoco()
        {
            using (var server = TestServer.Create(app =>
            {
                SwaggerConfig.Register(Config, "/autoPocoPath");
                app.UseWebApi(Config);
            }))
            {
                string result = server.HttpClient.GetStringAsync("autoPocoPath/swagger/ui/index").Result;
                Assert.True(result.Contains("<title>AutoPoco</title>"));
            }
        }

        [FactWithName]
        public void IncludeApiKeyInUi()
        {
            using (var server = TestServer.Create(app =>
            {
                SwaggerConfig.Register(Config, "/autoPocoPath");
                app.UseWebApi(Config);
            }))
            {
                string result = server.HttpClient.GetStringAsync("autoPocoPath/swagger/ui/index").Result;

                Assert.True(result.Contains("apiKeyName: 'Authorization'"));
                Assert.True(result.Contains("apiKeyIn: 'header'"));
            }
        }

        [FactWithName]
        public void SingApiVersionSetToV1AndAutoPoco()
        {
            using (var server = TestServer.Create(app =>
            {
                SwaggerConfig.Register(Config, "autoPocoPath");
                app.UseWebApi(Config);
            }))
            {
                string result = server.HttpClient.GetStringAsync("autoPocoPath/swagger/docs/v1").Result;
                var resultObj = JObject.Parse(result);

                var jObject = JObject.FromObject(new
                {
                    version = "v1",
                    title = "AutoPoco",
                });

                Assert.Equal(jObject.ToString(), resultObj["info"].ToString());
            }
        }

        [FactWithName]
        public void GroupByConvertsTagsToCamelCase()
        {
            using (var server = TestServer.Create(app =>
            {
                Config.MapHttpAttributeRoutes();
                SwaggerConfig.Register(Config, "autoPocoPath");
                app.UseWebApi(Config);
            }))
            {
                string result = server.HttpClient.GetStringAsync("autoPocoPath/swagger/docs/v1").Result;
                var resultObj = JObject.Parse(result);

                Assert.Equal("Table Definition", resultObj["paths"]["/api/{connectorName}/_definition/_table/{tableName}"]["get"]["tags"][0].ToString());
            }
        }

        [FactWithName]
        public void AddInOdataParameters()
        {
            var config = new HttpConfiguration();
            AutoPocoConfiguration.DashboardPathPrefix = null;
            using (var server = TestServer.Create(app =>
            {
                config.MapHttpAttributeRoutes();
                SwaggerConfig.Register(config, "autoPocoPath");
                app.UseWebApi(config);
            }))
            {
                string result = server.HttpClient.GetStringAsync("autoPocoPath/swagger/docs/v1").Result;
                var resultObj = JObject.Parse(result);

                var parameterNames = resultObj["paths"]["/api/{connectorName}/_table/{tableName}"]["get"]["parameters"].Select(c => c["name"]);

                Assert.Contains(parameterNames, c => c.Value<string>() == "$filter");
                Assert.Contains(parameterNames, c => c.Value<string>() == "$select");
                Assert.Contains(parameterNames, c => c.Value<string>() == "$expand");
                Assert.Contains(parameterNames, c => c.Value<string>() == "$orderby");
                Assert.Contains(parameterNames, c => c.Value<string>() == "$skip");
                Assert.Contains(parameterNames, c => c.Value<string>() == "$top");
                Assert.Contains(parameterNames, c => c.Value<string>() == "$apply");
                Assert.Contains(parameterNames, c => c.Value<string>() == "$count");
            }
        }

        [FactWithName]
        public void PullCommentsFromAutoPocoXmlUsingTableAsExample()
        {
            using (var server = TestServer.Create(app =>
            {
                Config.MapHttpAttributeRoutes();
                SwaggerConfig.Register(Config, "autoPocoPath");
                app.UseWebApi(Config);
            }))
            {
                string result = server.HttpClient.GetStringAsync("autoPocoPath/swagger/docs/v1").Result;
                var resultObj = JObject.Parse(result);

                Assert.Equal("Retrieve data from a given table", resultObj["paths"]["/api/{connectorName}/_table/{tableName}"]["get"]["summary"].Value<string>());
            }
        }
    }
}
