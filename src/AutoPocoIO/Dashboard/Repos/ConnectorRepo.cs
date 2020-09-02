using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace AutoPocoIO.Dashboard.Repos
{
    internal class ConnectorRepo : IConnectorRepo
    {
        private readonly AppDbContext _db;
        private readonly IConnectionStringFactory _factory;
        private readonly IEnumerable<IOperationResource> _resources;

        public ConnectorRepo(AppDbContext db, IConnectionStringFactory factory, IEnumerable<IOperationResource> resources)
        {
            _db = db;
            _factory = factory;
            _resources = resources;
        }

        ///<inheritdoc/>
        public virtual IEnumerable<ConnectorViewModel> ListConnectors()
        {
            return _db.Connector
                .Select(c => new ConnectorViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ResourceType = c.ResourceType,
                    DataSource = c.DataSource,
                    InitialCatalog = c.InitialCatalog,
                    Schema = c.Schema,
                    UserId = c.UserId,
                    IsActive = c.IsActive
                }).OrderBy(c => c.Name);
        }

        ///<inheritdoc/>
        public virtual int ConnectorCount() => _db.Connector.Count();

        ///<inheritdoc/>
        public string Save(ConnectorViewModel model)
        {
            var connectionInfo = new Resources.ConnectionInformation
            {
                InitialCatalog = model.InitialCatalog,
                DataSource = model.DataSource,
                UserId = model.UserId,
                Password = model.Password
            };
            model.ConnectionString = _factory.CreateConnectionString(model.ResourceType, connectionInfo);

            Connector connector = _db.Connector.Find(model.Id);

            connector.Name = model.Name;
            connector.ResourceType = model.ResourceType;
            connector.Schema = model.Schema;
            connector.ConnectionString = model.ConnectionString;
            connector.RecordLimit = model.RecordLimit.Value;
            connector.UserId = model.UserId;
            connector.InitialCatalog = model.InitialCatalog;
            connector.DataSource = model.DataSource;
            connector.Port = model.Port;
            connector.IsActive = model.IsActive;

            _db.SaveChanges();

            return connector.Id;
        }

        ///<inheritdoc/>
        public ConnectorViewModel GetById(string id)
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

            var connectionInfo = _factory.GetConnectionInformation(model.ResourceType, model.ConnectionStringDecrypted);
            model.InitialCatalog = connectionInfo.InitialCatalog;
            model.UserId = connectionInfo.UserId;
            model.DataSource = connectionInfo.DataSource;
            model.Password = connectionInfo.Password;

            return model;
        }

        ///<inheritdoc/>
        public string Insert(ConnectorViewModel model)
        {
            var connectionInfo = new Resources.ConnectionInformation
            {
                InitialCatalog = model.InitialCatalog,
                DataSource = model.DataSource,
                UserId = model.UserId,
                Password = model.Password
            };
            model.ConnectionString = _factory.CreateConnectionString(model.ResourceType, connectionInfo);

            Connector connector = new Connector
            {
                Name = model.Name,
                ResourceType = model.ResourceType,
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

            return connector.Id;
        }

        ///<inheritdoc/>
        public void Validate(ConnectorViewModel model, IDictionary<string, string> errors)
        {
            errors.Clear();
            if (string.IsNullOrEmpty(model.Name))
                errors[nameof(model.Name)] = "Connector Name is required.";
            else if (model.Name.Length > 50)
                errors[nameof(model.Name)] = $"Connector Name has a max length of 50.  {model.Name.Length} was submitted.";
            else if (_db.Connector.Any(c => c.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) && c.Id != model.Id))
            {
                errors[nameof(model.Name)] = $"The Connector Name {model.Name} already exists.";
            }

            if (string.IsNullOrEmpty(model.ResourceType))
                errors[nameof(model.ResourceType)] = "Resource Type is required.";

            if (string.IsNullOrEmpty(model.Schema))
                errors[nameof(model.Schema)] = "Schema Name is required.";
            else if (model.Schema.Length > 50)
                errors[nameof(model.Schema)] = $"Schema Name has a max length of 50.  {model.Schema.Length} was submitted.";

            if (string.IsNullOrEmpty(model.InitialCatalog))
                errors[nameof(model.InitialCatalog)] = "Database Name is required.";
            else if (model.InitialCatalog.Length > 50)
                errors[nameof(model.InitialCatalog)] = $"Database Name has a max length of 50.  {model.InitialCatalog.Length} was submitted.";

            if (model.UserId?.Length > 50)
                errors[nameof(model.UserId)] = $"User Id has a max length of 50.  {model.UserId.Length} was submitted.";

            if (string.IsNullOrEmpty(model.DataSource))
                errors[nameof(model.DataSource)] = "Server Name is required.";
            else if (model.DataSource.Length > 50)
                errors[nameof(model.DataSource)] = $"Server Name has a max length of 50.  {model.DataSource.Length} was submitted.";

            if (model.RecordLimit == null)
                errors[nameof(model.RecordLimit)] = "Record Limit is required.";

        }

        ///<inheritdoc/>
        public void Delete(string id)
        {
            var connector = _db.Connector.Find(id);
            _db.Connector.Remove(connector);
            _db.SaveChanges();
        }

        public IEnumerable<ResouceTypeViewModel> ListResoureTypes()
        {
            return _resources.Select(c => new ResouceTypeViewModel { ProviderName = c.ResourceType });
        }
    }
}
