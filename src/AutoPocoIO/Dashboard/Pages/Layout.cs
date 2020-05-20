using AutoPocoIO.Dashboard.Repo;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class Layout
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
