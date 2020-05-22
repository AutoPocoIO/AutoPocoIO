using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Models;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repo
{
    internal interface IConnectorRepo
    {
        int ConnectorCount();
        IEnumerable<Connector> ListConnectors();
        void Save(ConnectorViewModel model);
        ConnectorViewModel GetById(int id);
        void Insert(ConnectorViewModel model);
    }
}