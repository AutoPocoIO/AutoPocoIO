using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Models;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repos
{
    internal interface IDataDictionaryRepo
    {
        SchemaViewModel ListSchemaObject(string connectorId);
        TableDefinition ListTableDetails(string connectorId, string name);
        IEnumerable<NavigationPropertyViewModel> ListNavigationProperties(string connectorId, string name);
    }
}
