using Microsoft.Owin;
using System.Threading.Tasks;

namespace AutoPocoIO.AspNet.test.Owin
{
    public class EndOfPipeLineTestMiddleware : OwinMiddleware
    {
        public EndOfPipeLineTestMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public async override Task Invoke(IOwinContext context)
        {
            context.Response.ReasonPhrase = "OK";
            await context.Response.WriteAsync("end of pipeline").ConfigureAwait(false);
        }
    }
}
