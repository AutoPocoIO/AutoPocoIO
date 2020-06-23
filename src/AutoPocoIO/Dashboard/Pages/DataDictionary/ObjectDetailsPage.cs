using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class ObjectDetailsPage
    {
        private readonly IDataDictionaryRepo _repo;

        public ObjectDetailsPage(IDataDictionaryRepo repo, ILayoutPage layout)
        {
            _repo = repo;
            Layout = layout;
        }

        public virtual void ListTableDetails(int connectorId, string name)
        {
            ViewBag["model"] = _repo.ListTableDetails(connectorId, name);
            ViewBag["navs"] = _repo.ListNavigationProperties(connectorId, name);
        }
    }
}
