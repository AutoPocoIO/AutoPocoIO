using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AutoPocoIO.Middleware
{
    internal interface IMiddlewareRequest
    {
        string Method { get; }
        string Path { get; }
        string PathBase { get; }
        string LocalIpAddress { get; }
        string RemoteIpAddress { get; }
        string GetQuery(string key);
        Task<IDictionary<string, string[]>> ReadFormAsync();
        (T Entity, IDictionary<string, string> ErrorMessages) GetFormValues<T>() where T : class;
        Uri RequestUri { get; }
        Stream Body { get; }
    }
}
