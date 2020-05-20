using AutoPocoIO.Middleware.Dispatchers;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard
{
    internal interface IRazorForm
    {
        void SetForm(IDictionary<string, string[]> values);
        IMiddlewareDispatcher Save();
    }
}
