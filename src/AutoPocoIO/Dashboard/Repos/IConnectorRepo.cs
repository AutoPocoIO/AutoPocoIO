using AutoPocoIO.Dashboard.ViewModels;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repos
{
    public interface IConnectorRepo
    {
        int ConnectorCount();
        IEnumerable<ConnectorViewModel> ListConnectors();
        string Save(ConnectorViewModel model);
        ConnectorViewModel GetById(string id);
        string Insert(ConnectorViewModel model);
        void Validate(ConnectorViewModel model, IDictionary<string, string> errors);
        void Delete(string id);
    }
}