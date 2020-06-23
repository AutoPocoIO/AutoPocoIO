using AutoPocoIO.Dashboard.ViewModels;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repos
{
    public interface IConnectorRepo
    {
        int ConnectorCount();
        IEnumerable<ConnectorViewModel> ListConnectors();
        int Save(ConnectorViewModel model);
        ConnectorViewModel GetById(int id);
        int Insert(ConnectorViewModel model);
        void Validate(ConnectorViewModel model, IDictionary<string, string> errors);
        void Delete(int id);
    }
}