using AutoPocoIO.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Net.Http;

namespace AutoPocoIO.AspNetCore.test.Swagger
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class SwaggerConfigTests
    {
        private static readonly string dashPath = "autoPocoPath";
        HttpClient client;

#if NETCORE2_2
        private class TestStartup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSwaggerGen(SwaggerConfig.SwaggerServicesFunc);
                services.AddSwaggerExamplesFromAssemblyOf<SwaggerConfig>();
                services.AddMvcCore()
                   .AddApiExplorer();
            }
#pragma warning disable IDE0060 // Remove unused parameter
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                app.UseSwagger(SwaggerConfig.SwaggerAppBuilderFunc(dashPath));
                app.UseSwaggerUI(SwaggerConfig.SwaggerUIAppBuilderFunc(dashPath));
            }
        }
#else
        private class TestStartup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddRouting();
                services.AddSwaggerGen(SwaggerConfig.SwaggerServicesFunc);
                services.AddSwaggerExamplesFromAssemblyOf<SwaggerConfig>();
                services.AddMvcCore()
                   .AddApiExplorer();

                services.AddRazorPages();
            }
#pragma warning disable IDE0060, CS0618 // Remove unused parameter
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore IDE0060, CS0618 // Remove unused parameter
            {
                app.UseRouting();
                app.UseSwagger(SwaggerConfig.SwaggerAppBuilderFunc(dashPath));
                app.UseSwaggerUI(SwaggerConfig.SwaggerUIAppBuilderFunc(dashPath));

                app.UseStaticFiles();
                app.UseEndpoints(routes =>
                {
                    routes.MapControllers();
                    routes.MapRazorPages();
                });


            }
        }
#endif

        [TestInitialize]
        public void Init()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            TestServer testServer = new TestServer(builder);
            client = testServer.CreateClient();

            AutoPocoConfiguration.DashboardPathPrefix = null;
        }

        [TestMethod]
        public void RedirctToSwaggerPathStartsWithDashPath()
        {
            var resp = client.GetAsync("autoPocoPath/swagger").Result;
            Assert.AreEqual(301, (int)resp.StatusCode);
            Assert.AreEqual("swagger/index.html", resp.Headers.Location.ToString());
        }

        [TestMethod]
        public void VerifyDocumentTitleAutoPoco()
        {
            var result = client.GetStringAsync("autoPocoPath/swagger/index.html").Result;
            Assert.IsTrue(result.Contains("<title>AutoPoco</title>"));
        }


        [TestMethod]
        public void SingApiVersionSetToV1AndAutoPoco()
        {
            string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
            var resultObj = JObject.Parse(result);

            Assert.AreEqual("AutoPoco", resultObj["info"]["title"].ToString());
            Assert.AreEqual("v1", resultObj["info"]["version"].ToString());
        }

        //[TestMethod]
        //public void GroupByConvertsTagsToCamelCase()
        //{
        //    read();
        //    string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
        //    var resultObj = JObject.Parse(result);

        //    Assert.AreEqual("Table Definition", resultObj["paths"]["/api/{connectorName}/_definition/_table/{tableName}"]["get"]["tags"][0].ToString());
        //}

        //[TestMethod]
        //public void AddInOdataParameters()
        //{
        //    string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
        //    var resultObj = JObject.Parse(result);

        //    var parameterNames = resultObj["paths"]["/api/{connectorName}/_table/{tableName}"]["get"]["parameters"].Select(c => c["name"]);

        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$filter"));
        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$select"));
        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$expand"));
        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$orderby"));
        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$skip"));
        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$top"));
        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$apply"));
        //    Assert.IsTrue(parameterNames.Any(c => c.Value<string>() == "$count"));
        //}

        //[TestMethod]
        //public void PullCommentsFromcXmlUsingTableAsExample()
        //{
        //    string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
        //    var resultObj = JObject.Parse(result);

        //    Assert.AreEqual("Retrieve data from a given table", resultObj["paths"]["/api/{connectorName}/_table/{tableName}"]["get"]["summary"].Value<string>());
        //}

        [TestMethod]
        public void TagControllerToCamelCase()
        {
            var options = new SwaggerGenOptions();
            SwaggerConfig.SwaggerServicesFunc(options);

            var api = new ApiDescription();
            var descriptor = new ControllerActionDescriptor() { DisplayName = "name1", ControllerName = "MakeThisCamelCaseController" };
            api.ActionDescriptor = descriptor;
            var tags = options.SwaggerGeneratorOptions.TagsSelector(api);

            CollectionAssert.AreEqual(new[] { "Make This Camel Case Controller" }, tags.ToList());
        }

        [TestMethod]
        public void SkipTagIfNotController()
        {
            var options = new SwaggerGenOptions();
            SwaggerConfig.SwaggerServicesFunc(options);

            var api = new ApiDescription();
            var descriptor = new ActionDescriptor() { DisplayName = "name1" };
            api.ActionDescriptor = descriptor;
            var tags = options.SwaggerGeneratorOptions.TagsSelector(api);

            CollectionAssert.AreEqual(new string[0], tags.ToList());
        }
    }
}
