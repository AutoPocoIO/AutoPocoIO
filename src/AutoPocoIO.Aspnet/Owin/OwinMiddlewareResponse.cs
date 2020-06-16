using AutoPocoIO.Middleware;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Owin
{
    public class OwinMiddlewareResponse : IMiddlewareResponse
    {
        private readonly IOwinContext _context;

        public OwinMiddlewareResponse(IDictionary<string, object> environment)
        {
            _context = new OwinContext(environment);
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

        public void Redirect(string location)
        {
            _context.Response.Redirect(location);
        }

        public void SetExpire(DateTimeOffset? value)
        {
            _context.Response.Expires = value;
        }

        public Task WriteAsync(string text)
        {
            return _context.Response.WriteAsync(text);
        }
    }
}
