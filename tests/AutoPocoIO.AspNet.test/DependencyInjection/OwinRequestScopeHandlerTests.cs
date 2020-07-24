using AutoPoco.DependencyInjection;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Hosting;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class OwinRequestScopeHandlerTests
    {

        private class PublicHandler : RequestScopeFromOwinHandler
        {
            public PublicHandler()
            {
                InnerHandler = Mock.Of<HttpMessageHandler>();
            }
            public Task<HttpResponseMessage> SendAsyncPublic(HttpRequestMessage request)
            {
                return SendAsync(request, new System.Threading.CancellationToken());
            }
        }


        [TestMethod]
        public void PassContainerFromOwinToRequest()
        {
            var handler = new PublicHandler();
            var request = new HttpRequestMessage();
            var container = new Mock<IContainer>();
            container.Setup(c => c.GetService(typeof(Class1)))
                .Returns("container");

            var owinContext = new OwinContext();
            owinContext.Set("autopoco:container", container.Object);

            request.Properties["MS_OwinContext"] = owinContext;

            handler.SendAsyncPublic(request).Wait();

            AutoPocoDependencyScope result = (AutoPocoDependencyScope)request.Properties[HttpPropertyKeys.DependencyScope];
            Assert.AreEqual("container", result.GetService(typeof(Class1)));
        }
    }
}
