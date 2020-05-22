using AutoPocoIO.Dashboard.Repo;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class ConnectorsPage
    {
        private readonly IConnectorRepo _repo;

        public ConnectorsPage(IConnectorRepo repo, Layout layout)
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
