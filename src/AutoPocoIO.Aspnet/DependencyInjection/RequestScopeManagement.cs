using System;
using System.Web;

namespace AutoPoco.DependencyInjection
{
    internal class RequestScopeManagement : IHttpModule
    {
        public static IContainer ScopedContainer { get; set; }

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += Context_EndRequest;
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            if (ScopedContainer != null)
                ScopedContainer.Dispose();
        }
    }
}
