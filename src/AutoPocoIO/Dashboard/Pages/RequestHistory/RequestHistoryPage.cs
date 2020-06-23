using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    partial class RequestHistoryPage
    {
        private readonly IRequestHistoryRepo _requestHistoryRepo;

        public RequestHistoryPage(IRequestHistoryRepo requestHistoryRepo, ILayoutPage layout)
        {
            _requestHistoryRepo = requestHistoryRepo;
            Layout = layout;
        }


        public virtual void ListRequests() => ViewBag["requests"] = _requestHistoryRepo.ListRequest(50);
    }
}
