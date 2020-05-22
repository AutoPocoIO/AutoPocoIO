using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Models;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq;
using AutoPocoIO.Factories;

namespace AutoPocoIO.Dashboard.Repo
{
    internal class ConnectorRepo : IConnectorRepo
    {
        private readonly AppDbContext _db;
        private readonly IConnectionStringFactory _factory;

        public ConnectorRepo(AppDbContext db, IConnectionStringFactory factory)
        {
            _db = db;
            _factory = factory;
        }

        public virtual IEnumerable<Connector> ListConnectors() => _db.Connector;

        public virtual int ConnectorCount() => _db.Connector.Count();

        public void Save(ConnectorViewModel model)
        {
            model.ResourceType = 1;

            var connectionInfo = new AutoPocoIO.Resources.ConnectionInformation
            {
                InitialCatalog = model.InitialCatalog,
                DataSource = model.DataSource,
                UserId = model.UserId,
                Password = model.Password
            };
            model.ConnectionString = _factory.CreateConnectionString(model.ResourceType.Value, connectionInfo);

            Connector connector = _db.Connector.Find(model.Id);

            connector.Name = model.Name;
            connector.ResourceType = model.ResourceType.Value;
            connector.Schema = model.Schema;
            connector.ConnectionString = model.ConnectionString;
            connector.RecordLimit = model.RecordLimit.Value;
            connector.UserId = model.UserId;
            connector.InitialCatalog = model.InitialCatalog;
            connector.DataSource = model.DataSource;
            connector.Port = model.Port;

            _db.SaveChanges();

        }

        public ConnectorViewModel GetById(int id)
        {
            var model = _db.Connector.Select(c => new ConnectorViewModel
            {
                Id = c.Id,
                Name = c.Name,
                ResourceType = c.ResourceType,
                Schema = c.Schema,
                ConnectionString = c.ConnectionString,
                RecordLimit = c.RecordLimit,
                DataSource = c.DataSource,
                Port = c.Port

            }).Single(c => c.Id == id);

            var connectionInfo = _factory.GetConnectionInformation(model.ResourceType.Value, model.ConnectionStringDecrypted);
            model.InitialCatalog = connectionInfo.InitialCatalog;
            model.UserId = connectionInfo.UserId;
            model.DataSource = connectionInfo.DataSource;
            model.Password = connectionInfo.Password;

            return model;
        }

        public void Insert(ConnectorViewModel model)
        {
            model.ResourceType = 1;

            var connectionInfo = new AutoPocoIO.Resources.ConnectionInformation
            {
                InitialCatalog = model.InitialCatalog,
                DataSource = model.DataSource,
                UserId = model.UserId,
                Password = model.Password
            };
            model.ConnectionString = _factory.CreateConnectionString(model.ResourceType.Value, connectionInfo);

            Connector connector = new Connector
            {
                Name = model.Name,
                ResourceType = model.ResourceType.Value,
                Schema = model.Schema,
                ConnectionString = model.ConnectionString,
                RecordLimit = model.RecordLimit.Value,
                UserId = model.UserId,
                InitialCatalog = model.InitialCatalog,
                DataSource = model.DataSource,
                Port = model.Port,

            };

            _db.Connector.Add(connector);

            _db.SaveChanges();

           // return connector.Id;
        }
    }
}
