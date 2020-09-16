using AutoPocoIO.Dashboard.ViewModels;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repos
{
    /// <summary>
    /// Get information about request logs.
    /// </summary>
    public interface IRequestHistoryRepo
    {
        /// <summary>
        /// List requests information starting with the most recent.
        /// </summary>
        /// <param name="recordLimit">Number of records to return.</param>
        /// <returns></returns>
        IEnumerable<RequestGridViewModel> ListRequest(int recordLimit);
    }
}
