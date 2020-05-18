using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AutoPocoIO.AspNetCore.test.Middleware
{
    public class EndOfPipeLineTestMiddleware
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public EndOfPipeLineTestMiddleware(RequestDelegate next)
#pragma warning restore IDE0060 // Remove unused parameter
        {
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("end of pipeline").ConfigureAwait(false);
        }
    }
}
