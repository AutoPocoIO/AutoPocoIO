using System.Collections.Generic;

namespace AutoPocoIO.Services
{
    /// <summary>
    /// Access Http Request query strings.
    /// </summary>
    public interface IRequestQueryStringService
    {
        /// <summary>
        /// Format request query strings.
        /// </summary>
        /// <returns>Key/Value pair of the current request's query strings.</returns>
        IDictionary<string, string> GetQueryStrings();
    }
}
