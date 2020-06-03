using AutoPocoIO.Dashboard.Repo;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class SchemaPage
    {
        private readonly IDataDictionaryRepo _repo;

        public SchemaPage(IDataDictionaryRepo repo, Layout layout)
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
