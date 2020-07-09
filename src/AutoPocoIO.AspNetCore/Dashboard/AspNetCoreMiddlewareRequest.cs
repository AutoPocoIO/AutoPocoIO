using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    /// <summary>
    /// Represents the incoming side of an individual HTTP request.
    /// </summary>
    public class AspNetCoreMiddlewareRequest : IMiddlewareRequest
    {
        private readonly HttpContext _context;

        /// <summary>
        /// Initilize the middleware request from the <paramref name="context"/>
        /// </summary>
        /// <param name="context">HttpContext to set properties</param>
        public AspNetCoreMiddlewareRequest(HttpContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public string Method => _context.Request.Method;
        /// <inheritdoc/>
        public string Path => _context.Request.Path.Value;
        /// <inheritdoc/>
        public string PathBase => _context.Request.PathBase.Value;
        /// <inheritdoc/>
        public string LocalIpAddress => _context.Connection.LocalIpAddress.ToString();
        /// <inheritdoc/>
        public string RemoteIpAddress => _context.Connection.RemoteIpAddress.ToString();
        /// <inheritdoc/>
        public string GetQuery(string key) => _context.Request.Query[key];
        /// <inheritdoc/>
        public Uri RequestUri => new Uri(_context.Request.GetEncodedUrl());
        /// <inheritdoc/>
        public Stream Body => _context.Request.Body;
        /// <inheritdoc/>
        public async Task<IDictionary<string, string[]>> ReadFormAsync()
        {
            var form = await _context.Request.ReadFormAsync().ConfigureAwait(false);
            return form.ToDictionary(c => c.Key, c => c.Value.ToArray());
        }
    }
}