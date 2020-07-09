using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    /// <summary>
    /// Represents the outgoing side of an individual HTTP request.
    /// </summary>
    public class AspNetCoreMiddlewareResponse : IMiddlewareResponse
    {
        private readonly HttpContext _context;

        /// <summary>
        /// Initilize the middleware response from the <paramref name="context"/>
        /// </summary>
        /// <param name="context">HttpContext to set properties</param>
        public AspNetCoreMiddlewareResponse(HttpContext context)
        {
            _context = context;
        }

        ///<inheritdoc/>
        public string ContentType
        {
            get { return _context.Response.ContentType; }
            set { _context.Response.ContentType = value; }
        }
        ///<inheritdoc/>
        public int StatusCode
        {
            get { return _context.Response.StatusCode; }
            set { _context.Response.StatusCode = value; }
        }
        ///<inheritdoc/>
        public Stream Body => _context.Response.Body;
        ///<inheritdoc/>
        public Task WriteAsync(string text)
        {
            return _context.Response.WriteAsync(text);
        }
        ///<inheritdoc/>
        public void SetExpire(DateTimeOffset? value)
        {
            _context.Response.Headers["Expires"] = value?.ToString("r", CultureInfo.InvariantCulture);
        }
        ///<inheritdoc/>
        public void Redirect(string location)
        {
            _context.Response.Redirect(location);
        }
    }
}