using System;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    internal interface IMiddlewareResponse
    {
       string ContentType { get; set; }
       int StatusCode { get; set; }
       Stream Body { get; }
       void SetExpire(DateTimeOffset? value);
       Task WriteAsync(string text);
       void Redirect(string location);
    }
}