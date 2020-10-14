using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;
using System;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class ObjectDetailsPage
    {
        private readonly IDataDictionaryRepo _repo;

        public ObjectDetailsPage(IDataDictionaryRepo repo, ILayoutPage layout)
              : base(layout, "Data Dictionary - AutoPoco")
        {
            _repo = repo;
        }

        public virtual void ListTableDetails(Guid connectorId, string name)
        {
            ViewBag["model"] = _repo.ListTableDetails(connectorId, name);
            ViewBag["navs"] = _repo.ListNavigationProperties(connectorId, name);
        }
    }
}
