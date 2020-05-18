using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    internal sealed class AspNetCoreMiddlewareResponse : IMiddlewareResponse
    {
        private readonly HttpContext _context;

        public AspNetCoreMiddlewareResponse(HttpContext context)
        {
            _context = context;
        }

        public string ContentType
        {
            get { return _context.Response.ContentType; }
            set { _context.Response.ContentType = value; }
        }

        public int StatusCode
        {
            get { return _context.Response.StatusCode; }
            set { _context.Response.StatusCode = value; }
        }

        public Stream Body => _context.Response.Body;

        public Task WriteAsync(string text)
        {
            return _context.Response.WriteAsync(text);
        }

        public void SetExpire(DateTimeOffset? value)
        {
            _context.Response.Headers["Expires"] = value?.ToString("r", CultureInfo.InvariantCulture);
        }

        public void Redirect(string location)
        {
            _context.Response.Redirect(location);
        }
    }
}