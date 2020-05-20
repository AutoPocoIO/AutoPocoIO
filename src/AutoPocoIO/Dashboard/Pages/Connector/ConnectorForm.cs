﻿using AutoPocoIO.Constants;
using AutoPocoIO.Dashboard.Extensions;
using AutoPocoIO.Dashboard.Repo;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Middleware.Dispatchers;
using AutoPocoIO.Models;
using AutoPocoIO.Services;
using System.Collections.Generic;
using static AutoPocoIO.AutoPocoConstants;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class ConnectorForm: IRazorForm
    {
        private readonly IConnectorRepo _repo;
        private Connector model;

        public ConnectorForm(IConnectorRepo repo, Layout layout)
        {
            _repo = repo;
            Layout = layout;
        }

        public virtual IMiddlewareDispatcher Save()
        {
            if (Validate())
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

            return new RazorPageDispatcher(c => this);
        }

        private bool Validate()
        {
            return true;
        }

        public virtual void GetById(int id)
        {
            ViewBag["model"] = _repo.GetById(id);
        }

        public void SetForm(IDictionary<string, string[]> values)
        {
            model = new Connector()
            {
                Id = values.FindValue<int>("id"),
                Name = values.FindValue<string>("connectorName"),
                DataSource = values.FindValue<string>("serverName")
            };
        }

        public void NewConnector()
        {
            ViewBag["model"] = new Connector();
        }
    }
}