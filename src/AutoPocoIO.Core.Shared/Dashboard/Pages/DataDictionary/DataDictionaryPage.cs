﻿using AutoPocoIO.Dashboard.Repos;
using AutoPocoIO.Middleware;

namespace AutoPocoIO.Dashboard.Pages
{
    internal partial class DataDictionaryPage
    {
        private readonly IConnectorRepo _repo;

        public DataDictionaryPage(IConnectorRepo repo, ILayoutPage layout)
             : base(layout, "Data Dictionary - AutoPoco")
        {
            _repo = repo;
        }

        public virtual void ListConnectors()
        {
            ViewBag["Connectors"] = _repo.ListConnectors();
        }
    }
}
