using AutoPocoIO.Middleware.Dispatchers;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard
{
    /// <summary>
    /// Page containing a form.
    /// </summary>
    public interface IRazorForm
    {
        /// <summary>
        /// Read request form and set to dictionary
        /// </summary>
        /// <param name="values">Result dictionary</param>
        void SetForm(IDictionary<string, string[]> values);
        /// <summary>
        /// Execute command.
        /// </summary>
        /// <returns>Result dispatcher.</returns>
        IMiddlewareDispatcher Save();
    }
}
