using AutoPocoIO.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace AutoPocoIO.AspNetCore.test.Swagger
{
    
    [Trait("Category", TestCategories.Unit)]
    public class SwaggerConfigTests
    {
        private static readonly string dashPath = "autoPocoPath";
        readonly HttpClient client;

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

        public SwaggerConfigTests()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            TestServer testServer = new TestServer(builder);
            client = testServer.CreateClient();

            AutoPocoConfiguration.DashboardPathPrefix = null;
        }

        [FactWithName]
        public void RedirctToSwaggerPathStartsWithDashPath()
        {
            var resp = client.GetAsync("autoPocoPath/swagger").Result;
            Assert.Equal(301, (int)resp.StatusCode);
            Assert.Equal("swagger/index.html", resp.Headers.Location.ToString());
        }

        [FactWithName]
        public void VerifyDocumentTitleAutoPoco()
        {
            var result = client.GetStringAsync("autoPocoPath/swagger/index.html").Result;
            Assert.Contains("<title>AutoPoco</title>", result);
        }


        [FactWithName]
        public void SingApiVersionSetToV1AndAutoPoco()
        {
            string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
            var resultObj = JObject.Parse(result);

            var jObject = JObject.FromObject(new
            {
                version = "v1",
                title = "AutoPoco",
            });

            Assert.Equal(jObject.ToString(), resultObj["info"].ToString());
        }

        [FactWithName]
        public void GroupByConvertsTagsToCamelCase()
        {
            string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
            var resultObj = JObject.Parse(result);

            Assert.Equal("Table Definition", resultObj["paths"]["/api/{connectorName}/_definition/_table/{tableName}"]["get"]["tags"][0].ToString());
        }

        [FactWithName]
        public void AddInOdataParameters()
        {
            string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
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

        [FactWithName]
        public void PullCommentsFromcXmlUsingTableAsExample()
        {
            string result = client.GetStringAsync("autoPocoPath/swagger/v1/swagger.json").Result;
            var resultObj = JObject.Parse(result);

            Assert.Equal("Retrieve data from a given table", resultObj["paths"]["/api/{connectorName}/_table/{tableName}"]["get"]["summary"].Value<string>());
        }

        [FactWithName]
        public void TagControllerToCamelCase()
        {
            var options = new SwaggerGenOptions();
            SwaggerConfig.SwaggerServicesFunc(options);

            var api = new ApiDescription();
            var descriptor = new ControllerActionDescriptor() { DisplayName = "name1", ControllerName = "MakeThisCamelCaseController" };
            api.ActionDescriptor = descriptor;
            var tags = options.SwaggerGeneratorOptions.TagsSelector(api);

            Assert.Equal(new[] { "Make This Camel Case Controller" }, tags.ToList());
        }

        [FactWithName]
        public void SkipTagIfNotController()
        {
            var options = new SwaggerGenOptions();
            SwaggerConfig.SwaggerServicesFunc(options);

            var api = new ApiDescription();
            var descriptor = new ActionDescriptor() { DisplayName = "name1" };
            api.ActionDescriptor = descriptor;
            var tags = options.SwaggerGeneratorOptions.TagsSelector(api);

            Assert.Equal(new string[0], tags.ToList());
        }
    }
}
