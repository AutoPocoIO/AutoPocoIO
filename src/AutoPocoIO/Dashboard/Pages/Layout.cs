using AutoPocoIO.Dashboard.Repo;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class Layout : ILayoutPage
    {
        private readonly IConnectorRepo _repo;

        public Layout(IConnectorRepo repo)
        {
            _repo = repo;
        }

        public virtual int ConnectorCount()
        {
            return _repo.ConnectorCount();
        }
    }
}
