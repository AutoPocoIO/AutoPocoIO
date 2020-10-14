using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;
using System;

namespace AutoPocoIO.Dashboard.Pages
{
    /// <summary>
    /// Connector list page.
    /// </summary>
    public partial class ConnectorsPage
    {
        private readonly IConnectorRepo _repo;

        /// <summary>
        /// Initialize Connector page.
        /// </summary>
        /// <param name="repo">Database access.</param>
        /// <param name="layout">Unified layout.</param>
        public ConnectorsPage(IConnectorRepo repo, ILayoutPage layout)
             : base(layout, "Connectors - AutoPoco")
        {
            _repo = repo;
        }

        /// <summary>
        /// Get all connectors and set viewbag
        /// </summary>
        public virtual void ListConnectors()
        {
            ViewBag["Connectors"] = _repo.ListConnectors();
        }
        /// <summary>
        /// Remove connector
        /// </summary>
        /// <param name="id">Connector Id</param>
        public virtual void Delete(Guid id) => _repo.Delete(id);
    }
}
