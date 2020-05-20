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

        public void Save(Connector model)
        {
            var entity = _db.Connector.Find(model.Id);

            entity.Name = model.Name;
            entity.DataSource = model.DataSource;



            _db.SaveChanges();
        }

        public Connector GetById(int id) => _db.Connector.Find(id);

        public void Insert(Connector model)
        {
            _db.Connector.Add(model);
            _db.SaveChanges();
        }
    }
}
