using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using System.Collections.Generic;
using System.Reflection;

namespace AutoPocoIO.Dashboard.Repo
{
    internal class DataDictionaryRepo : IDataDictionaryRepo
    {
        private readonly IResourceFactory _resourceFactory;

        public DataDictionaryRepo(IResourceFactory resourceFactory)
        {
            _resourceFactory = resourceFactory;
        }

        public SchemaViewModel ListSchemaObject(int id)
        {
            var resource = _resourceFactory.GetResource(id, string.Empty);
            resource.LoadSchema();

            return new SchemaViewModel()
            {
                ConnectorId = id,
                ConnectorName = resource.Connector.Name,
                Tables = resource.DbSchema.Tables,
                Views = resource.DbSchema.Views,
                StoredProcedures = resource.DbSchema.StoredProcedures
            };
        }

        public TableDefinition ListTableDetails(int connectorId, string name)
        {
            var resource = _resourceFactory.GetResource(connectorId, OperationType.Any, name);
            return resource.GetTableDefinition();
        }

        public IEnumerable<NavigationPropertyViewModel> ListNavigationProperties(int connectorId, string name)
        {
            var resource = _resourceFactory.GetResource(connectorId, OperationType.Any, name);
            var data = resource.GetResourceRecords(new Dictionary<string, string>());

            var properties = new List<NavigationPropertyViewModel>();
            foreach(var property in data.ElementType.GetProperties())
            {
                if(property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    NavigationPropertyViewModel navigationProperty = new NavigationPropertyViewModel()
                    {
                        Name = property.Name,
                        ReferencedSchema = property.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName,
                        ReferencedTable = property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName
                    };
                    if(typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        navigationProperty.Relationship = "Many to 1";
                    }
                    else
                    {
                        navigationProperty.Relationship = "1 to Many";
                    }

                    properties.Add(navigationProperty);
                }
            }

            return properties;

        }
    }
}
