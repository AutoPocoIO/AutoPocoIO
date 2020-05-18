using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoPocoIO.Api
{
    /// <summary>
    /// View all schema objects
    /// </summary>
    public class SchemaOperations : ISchemaOperations
    {

        private readonly IResourceFactory _resourceFactory;
        public SchemaOperations(IServiceProvider serviceProvider)
        {
            _resourceFactory = serviceProvider.GetRequiredService<IResourceFactory>();
        }

        /// <summary>
        /// List of objects in the database by type
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="loggingService">Pass in logging service if the request needs to be logged</param>
        /// <returns></returns>
        public SchemaDefinition Definition(string connectorName, ILoggingService loggingService = null)
        {
            loggingService?.AddSchemaToLogger(connectorName);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, string.Empty);
            return resource.GetSchemaDefinition();
        }
    }
}
