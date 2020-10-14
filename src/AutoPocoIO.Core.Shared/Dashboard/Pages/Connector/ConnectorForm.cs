using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard.Extensions;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;
using System;
using System.Collections.Generic;
using static AutoPocoIO.AutoPocoConstants;

namespace AutoPocoIO.Dashboard.Pages
{
    /// <summary>
    /// Connector form page.
    /// </summary>
    public partial class ConnectorForm : IRazorForm
    {
        private readonly IConnectorRepo _repo;
        private ConnectorViewModel model;
        private readonly IDictionary<string, string> errors;

        /// <summary>
        /// Initialize Connector page.
        /// </summary>
        /// <param name="repo">Database access.</param>
        /// <param name="layout">Unified layout.</param>
        public ConnectorForm(IConnectorRepo repo, ILayoutPage layout)
            : base(layout, "Connectors - AutoPoco")
        {
            _repo = repo;
            errors = new Dictionary<string, string>();
            ViewBag["errors"] = errors;
        }

        /// <summary>
        /// Add or Update a connector.
        /// </summary>
        /// <returns>Redirect to edit page or display errors.</returns>
        public virtual IMiddlewareDispatcher Save()
        {
            _repo.Validate(model, errors);
            if (errors.Count == 0)
            {
                Guid id;
                if (model.Id == null)
                {
                    LoggingService.AddTableToLogger(DefaultConnectors.AppDB, DefaultTables.Connectors, HttpMethodType.POST);
                    id = _repo.Insert(model);
                }
                else
                {
                    LoggingService.AddTableToLogger(DefaultConnectors.AppDB, DefaultTables.Connectors, HttpMethodType.PUT);
                    id = _repo.Save(model);
                }

                return new RedirectDispatcher($"/Connectors/Connector/{id}");
            }

            ViewBag["resourceTypes"] = _repo.ListResoureTypes();
            ViewBag["model"] = model;
            return new RazorPageDispatcher(this);
        }

        /// <summary>
        /// Get connector by id and set viewbag
        /// </summary>
        /// <param name="id">Connector id</param>
        public virtual void GetById(Guid id)
        {
            ViewBag["resourceTypes"] = _repo.ListResoureTypes();
            ViewBag["model"] = _repo.GetById(id);
        }
        ///<inheritdoc/>
        public virtual void SetForm(IDictionary<string, string[]> values)
        {
            model = new ConnectorViewModel()
            {
                Id = values.FindValue<Guid?>("id"),
                ResourceType = values.FindValue<string>("resourceType"),
                Name = values.FindValue<string>("connectorName"),
                DataSource = values.FindValue<string>("serverName"),
                InitialCatalog = values.FindValue<string>("databaseName"),
                Schema = values.FindValue<string>("schema"),
                UserId = values.FindValue<string>("userId"),
                Password = values.FindValue<string>("password"),
                RecordLimit = values.FindValue<int?>("recordLimit"),
                IsActive = values.FindValue<bool>("isEnabled")
            };
        }
        /// <summary>
        /// Create new connector and set view bag
        /// </summary>
        public virtual void NewConnector()
        {
            ViewBag["resourceTypes"] = _repo.ListResoureTypes();
            ViewBag["model"] = new ConnectorViewModel { IsActive = true };
        }
    }
}
