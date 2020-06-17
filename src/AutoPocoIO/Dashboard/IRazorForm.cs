using AutoPocoIO.Middleware.Dispatchers;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard
{
    public interface IRazorForm
    {
        void SetForm(IDictionary<string, string[]> values);
        IMiddlewareDispatcher Save();
    }
}
