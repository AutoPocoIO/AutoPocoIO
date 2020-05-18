using AutoPocoIO.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.AspNetCore.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class ServiceCollectionExtensionTests
    {
        private class TestStartup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddAutoPoco();

            }
#pragma warning disable IDE0060 // Remove unused parameter
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#pragma warning restore IDE0060 // Remove unused parameter
            {
            }
        }

        [TestMethod]
        public void OutputFormatsAddsOdataForSwagger()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            TestServer testServer = new TestServer(builder);

            var options = testServer.Host.Services.GetRequiredService<IConfigureOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();
            mvcOptions.OutputFormatters.Add(new ODataOutputFormatter(new List<ODataPayloadKind>()));

            options.Configure(mvcOptions);

            var formater = (ODataOutputFormatter)mvcOptions.OutputFormatters.First();

            Assert.AreEqual(1, formater.SupportedMediaTypes.Count());
            Assert.AreEqual("application/prs.odatatestxx-odata", formater.SupportedMediaTypes[0].ToString());
        }

        [TestMethod]
        public void InputFormatsAddsOdataForSwagger()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            TestServer testServer = new TestServer(builder);

            var options = testServer.Host.Services.GetRequiredService<IConfigureOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();
            mvcOptions.InputFormatters.Add(new ODataInputFormatter(new List<ODataPayloadKind>()));

            options.Configure(mvcOptions);

            var formater = (ODataInputFormatter)mvcOptions.InputFormatters.First();

            Assert.AreEqual(1, formater.SupportedMediaTypes.Count());
            Assert.AreEqual("application/prs.odatatestxx-odata", formater.SupportedMediaTypes[0].ToString());
        }

        [TestMethod]
        public void OutputFormatsSkipsOfOdataFormatExists()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            TestServer testServer = new TestServer(builder);

            var options = testServer.Host.Services.GetRequiredService<IConfigureOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();

            var format = new ODataOutputFormatter(new List<ODataPayloadKind>());
            format.SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/plain"));
            mvcOptions.OutputFormatters.Add(format);

            options.Configure(mvcOptions);

            var formater = (ODataOutputFormatter)mvcOptions.OutputFormatters.First();

            Assert.AreEqual(1, formater.SupportedMediaTypes.Count());
            Assert.AreEqual("text/plain", formater.SupportedMediaTypes[0].ToString());
        }

        [TestMethod]
        public void InputFormatsSkipsOfOdataFormatExists()
        {
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            TestServer testServer = new TestServer(builder);

            var options = testServer.Host.Services.GetRequiredService<IConfigureOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();

            var format = new ODataInputFormatter(new List<ODataPayloadKind>());
            format.SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/plain"));
            mvcOptions.InputFormatters.Add(format);

            options.Configure(mvcOptions);

            var formater = (ODataInputFormatter)mvcOptions.InputFormatters.First();

            Assert.AreEqual(1, formater.SupportedMediaTypes.Count());
            Assert.AreEqual("text/plain", formater.SupportedMediaTypes[0].ToString());
        }
    }
}
