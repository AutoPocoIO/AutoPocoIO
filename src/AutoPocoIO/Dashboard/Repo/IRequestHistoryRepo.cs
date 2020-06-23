using AutoPocoIO.Dashboard.ViewModels;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repo
{
    public interface IRequestHistoryRepo
    {
        IEnumerable<RequestGridViewModel> ListRequest(int recordLimit);
    }
}
