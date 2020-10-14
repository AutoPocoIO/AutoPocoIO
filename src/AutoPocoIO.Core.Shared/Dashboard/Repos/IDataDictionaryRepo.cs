using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Models;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.Repos
{
    internal interface IDataDictionaryRepo
    {
        SchemaViewModel ListSchemaObject(Guid connectorId);
        TableDefinition ListTableDetails(Guid connectorId, string name);
        IEnumerable<NavigationPropertyViewModel> ListNavigationProperties(Guid connectorId, string name);
    }
}
