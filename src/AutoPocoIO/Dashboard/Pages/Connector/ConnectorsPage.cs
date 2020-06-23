using AutoPocoIO.Dashboard.Repo;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    public partial class ConnectorsPage
    {
        private readonly IConnectorRepo _repo;

        public ConnectorsPage(IConnectorRepo repo, ILayoutPage layout)
        {
            _repo = repo;
            Layout = layout;
        }

        public virtual void ListConnectors()
        {
            ViewBag["Connectors"] = _repo.ListConnectors();
        }

        public virtual void Delete(int id) => _repo.Delete(id);
    }
}
