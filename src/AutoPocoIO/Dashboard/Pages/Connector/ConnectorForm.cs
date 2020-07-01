using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard.Extensions;
using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Middleware;
using AutoPocoIO.Middleware.Dispatchers;
using System.Collections.Generic;
using static AutoPocoIO.AutoPocoConstants;

namespace AutoPocoIO.Dashboard.Pages
{
    public partial class ConnectorForm: IRazorForm
    {
        private readonly IConnectorRepo _repo;
        private ConnectorViewModel model;
        private IDictionary<string, string> errors;

        public ConnectorForm(IConnectorRepo repo, ILayoutPage layout)
        {
            _repo = repo;
            Layout = layout;
            errors = new Dictionary<string, string>();
            ViewBag["errors"] = errors;
        }

        public virtual IMiddlewareDispatcher Save()
        {
            _repo.Validate(model, errors);
            if (errors.Count == 0)
            {
                string id;
                if (string.IsNullOrEmpty(model.Id))
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

            ViewBag["model"] = model;
            return new RazorPageDispatcher(c => this);
        }

        public virtual void GetById(string id)
        {
            ViewBag["model"] = _repo.GetById(id);
        }

        public void SetForm(IDictionary<string, string[]> values)
        {
            model = new ConnectorViewModel()
            {
                Id = values.FindValue<string>("id"),
                ResourceType = 1,
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

        public void NewConnector()
        {
            ViewBag["model"] = new ConnectorViewModel();
        }
    }
}
