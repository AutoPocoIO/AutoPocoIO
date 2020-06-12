using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;

namespace AutoPoco.DependencyInjection
{
    internal class RequestScopeFromOwinHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var owinContext = request.GetOwinContext();
            IContainer container = owinContext.Get<IContainer>("autopoco:container");

            var dependencyScope = new AutoPocoDependencyScope(container);
            request.Properties[HttpPropertyKeys.DependencyScope] = dependencyScope;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
