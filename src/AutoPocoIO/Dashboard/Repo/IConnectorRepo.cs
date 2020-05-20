using AutoPocoIO.Models;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repo
{
    internal interface IConnectorRepo
    {
        int ConnectorCount();
        IEnumerable<Connector> ListConnectors();
        void Save(Connector model);
        Connector GetById(int id);
        void Insert(Connector model);
    }
}