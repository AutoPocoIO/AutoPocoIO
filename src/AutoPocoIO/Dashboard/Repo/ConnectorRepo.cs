using AutoPocoIO.Context;
using AutoPocoIO.Models;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;

namespace AutoPocoIO.Dashboard.Repo
{
    internal class ConnectorRepo : IConnectorRepo
    {
        private readonly AppDbContext _db;

        public ConnectorRepo(AppDbContext db)
        {
            _db = db;
        }

        public virtual IEnumerable<Connector> ListConnectors() => _db.Connector;

        public virtual int ConnectorCount() => _db.Connector.Count();
    }
}
