using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Models;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq;
using AutoPocoIO.Factories;
using System;

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

            var connectionInfo = new Resources.ConnectionInformation
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
            connector.IsActive = model.IsActive;

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
                Port = c.Port,
                IsActive = c.IsActive

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

            var connectionInfo = new Resources.ConnectionInformation
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
                IsActive = model.IsActive
            };

            _db.Connector.Add(connector);

            _db.SaveChanges();
        }

        public void Validate(ConnectorViewModel model, IDictionary<string, string> errors)
        {
            errors.Clear();
            if (string.IsNullOrEmpty(model.Name))
                errors[nameof(model.Name)] = "Connector Name is required.";
            else if(model.Name.Length > 50)
                errors[nameof(model.Name)] = $"Connector Name has a max length of 50.  {model.Name.Length} was submitted.";
            else if(_db.Connector.Any(c => c.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) && c.Id != model.Id))
            {
                errors[nameof(model.Name)] =  $"The Connector Name {model.Name} already exists.";
            }

            if (model.ResourceType == null)
                errors[nameof(model.ResourceType)] = "Resource Type is required.";

            if (string.IsNullOrEmpty(model.Schema))
                errors[nameof(model.Schema)] = "Schema Name is required.";
            else if (model.Schema.Length > 50)
                errors[nameof(model.Schema)] = $"Schema Name has a max length of 50.  {model.Schema.Length} was submitted.";

            if (string.IsNullOrEmpty(model.InitialCatalog))
                errors[nameof(model.InitialCatalog)] = "Database Name is required.";
            else if (model.InitialCatalog.Length > 50)
                errors[nameof(model.InitialCatalog)] = $"Database Name has a max length of 50.  {model.InitialCatalog.Length} was submitted.";

            if (model.UserId.Length > 50)
                errors[nameof(model.UserId)] = $"Database Name has a max length of 50.  {model.UserId.Length} was submitted.";

            if (string.IsNullOrEmpty(model.DataSource))
                errors[nameof(model.DataSource)] = "Server Name is required.";
            else if (model.DataSource.Length > 50)
                errors[nameof(model.DataSource)] = $"Server Name has a max length of 50.  {model.DataSource.Length} was submitted.";

            if (model.RecordLimit == null)
                errors[nameof(model.RecordLimit)] = "Record Limit is required.";

        }
    }
}
