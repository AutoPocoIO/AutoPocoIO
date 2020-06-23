using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Models;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repos
{
    internal interface IDataDictionaryRepo
    {
        SchemaViewModel ListSchemaObject(int connectorId);
        TableDefinition ListTableDetails(int connectorId, string name);
        IEnumerable<NavigationPropertyViewModel> ListNavigationProperties(int connectorId, string name);
    }
}
