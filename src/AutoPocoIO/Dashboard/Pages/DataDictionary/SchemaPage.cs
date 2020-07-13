using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class SchemaPage
    {
        private readonly IDataDictionaryRepo _repo;

        public SchemaPage(IDataDictionaryRepo repo, ILayoutPage layout)
              : base(layout, "Data Dictionary - AutoPoco")
        {
            _repo = repo;
        }

        public virtual void ListDbObjects(string connectorId)
        {
            ViewBag["model"] = _repo.ListSchemaObject(connectorId);
        }
    }
}
