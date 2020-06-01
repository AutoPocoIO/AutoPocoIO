using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard.Extensions;
using AutoPocoIO.Dashboard.Repo;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Middleware.Dispatchers;
using System.Collections.Generic;
using static AutoPocoIO.AutoPocoConstants;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class ConnectorForm: IRazorForm
    {
        private readonly IConnectorRepo _repo;
        private ConnectorViewModel model;
        private readonly IDictionary<string, string> errors;

        public ConnectorForm(IConnectorRepo repo, Layout layout)
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
                if (model.Id == 0)
                {
                    LoggingService.AddTableToLogger(DefaultConnectors.AppDB, DefaultTables.Connectors, HttpMethodType.POST);
                    _repo.Insert(model);
                }
                else
                {
                    LoggingService.AddTableToLogger(DefaultConnectors.AppDB, DefaultTables.Connectors, HttpMethodType.PUT);
                    _repo.Save(model);
                }

                return new RedirectDispatcher("/Connectors");
            }

            ViewBag["model"] = model;
            return new RazorPageDispatcher(c => this);
        }

        public virtual void GetById(int id)
        {
            ViewBag["model"] = _repo.GetById(id);
        }

        public void SetForm(IDictionary<string, string[]> values)
        {
            model = new ConnectorViewModel()
            {
                Id = values.FindValue<int?>("id"),
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
