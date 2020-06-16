using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    public class AspNetCoreMiddlewareRequest : IMiddlewareRequest
    {
        private readonly HttpContext _context;
        public AspNetCoreMiddlewareRequest(HttpContext context)
        {
            _context = context;
        }

        public string Method => _context.Request.Method;
        public string Path => _context.Request.Path.Value;
        public string PathBase => _context.Request.PathBase.Value;
        public string LocalIpAddress => _context.Connection.LocalIpAddress.ToString();
        public string RemoteIpAddress => _context.Connection.RemoteIpAddress.ToString();
        public string GetQuery(string key) => _context.Request.Query[key];
        public Uri RequestUri => new Uri(_context.Request.GetEncodedUrl());
        public Stream Body => _context.Request.Body;

        public async Task<IDictionary<string, string[]>> ReadFormAsync()
        {
            var form = await _context.Request.ReadFormAsync().ConfigureAwait(false);
            return form.ToDictionary(c => c.Key, c => c.Value.ToArray());
        }
    }
}