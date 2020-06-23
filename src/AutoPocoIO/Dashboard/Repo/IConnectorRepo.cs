using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Models;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repo
{
    public interface IConnectorRepo
    {
        int ConnectorCount();
        IEnumerable<ConnectorViewModel> ListConnectors();
        void Save(ConnectorViewModel model);
        ConnectorViewModel GetById(int id);
        void Insert(ConnectorViewModel model);
        void Validate(ConnectorViewModel model, IDictionary<string, string> errors);
        void Delete(int id);
    }
}