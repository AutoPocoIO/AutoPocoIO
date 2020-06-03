using AutoPocoIO.Dashboard.Repo;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class DataDictionaryPage
    {
        private readonly IConnectorRepo _repo;

        public DataDictionaryPage(IConnectorRepo repo, Layout layout)
        {
            _repo = repo;
            Layout = layout;
        }

        public virtual void ListConnectors()
        {
            ViewBag["Connectors"] = _repo.ListConnectors();
        }
    }
}
