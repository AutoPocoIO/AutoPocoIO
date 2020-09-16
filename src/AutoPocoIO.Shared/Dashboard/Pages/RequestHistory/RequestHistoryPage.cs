using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    /// <summary>
    /// Reqeust list page.
    /// </summary>
    partial class RequestHistoryPage
    {
        private readonly IRequestHistoryRepo _requestHistoryRepo;

        /// <summary>
        /// Initialize request page.
        /// </summary>
        /// <param name="requestHistoryRepo">Database access.</param>
        /// <param name="layout">Unified layout.</param>
        public RequestHistoryPage(IRequestHistoryRepo requestHistoryRepo, ILayoutPage layout)
             : base(layout, "Request History - AutoPoco")
        {
            _requestHistoryRepo = requestHistoryRepo;
        }

        /// <summary>
        /// Set last 50 requests to the view bag.
        /// </summary>
        public virtual void ListRequests() => ViewBag["requests"] = _requestHistoryRepo.ListRequest(50);
    }
}
