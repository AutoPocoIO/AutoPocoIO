using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class SchemaPage
    {
        private readonly IDataDictionaryRepo _repo;

        public SchemaPage(IDataDictionaryRepo repo, ILayoutPage layout)
        {
            _repo = repo;
            Layout = layout;
        }

        public virtual void ListDbObjects(int connectorId)
        {
            ViewBag["model"] = _repo.ListSchemaObject(connectorId);
        }
    }
}
