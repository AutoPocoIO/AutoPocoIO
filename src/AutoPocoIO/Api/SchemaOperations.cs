using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;

namespace AutoPocoIO.Api
{
    /// <summary>
    /// View all schema objects
    /// </summary>
    public class SchemaOperations : ISchemaOperations
    {

        private readonly IResourceFactory _resourceFactory;
        /// <summary>
        /// Initialize schema operations with access to all registered resource types.
        /// </summary>
        /// <param name="resourceFactory">Get resource from the connector.</param>
        public SchemaOperations(IResourceFactory resourceFactory)
        {
            _resourceFactory = resourceFactory;
        }

        /// <inheritdoc />
        public SchemaDefinition Definition(string connectorName, ILoggingService loggingService = null)
        {
            loggingService?.AddSchemaToLogger(connectorName);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, string.Empty);
            return resource.GetSchemaDefinition();
        }
    }
}
