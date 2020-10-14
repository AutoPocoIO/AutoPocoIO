using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AutoPocoIO.Factories
{
    /// <summary>
    /// Search for a resouce by connector.
    /// </summary>
    public class ResourceFactory : IResourceFactory
    {
        private readonly IAppAdminService _appAdminService;
        private readonly IEnumerable<IOperationResource> _resources;

        /// <summary>
        /// Initialize factory with a registered resouce types.
        /// </summary>
        public ResourceFactory(IAppAdminService appAdminService, IEnumerable<IOperationResource> resources)
        {
            _resources = resources;
            _appAdminService = appAdminService;
        }

        ///<inheritdoc/>
        public IOperationResource GetResource(string connectorName, OperationType dbAction, string dbObjectName)
        {
            var connector = _appAdminService.GetConnection(connectorName);
            return GetResourceType(connector, dbAction, dbObjectName);
        }
        ///<inheritdoc/>
        public IOperationResource GetResource(Guid connectorId, string dbObjectName)
        {
            var connector = _appAdminService.GetConnectionById(connectorId);
            return GetResourceType(connector, OperationType.read, dbObjectName);
        }

        ///<inheritdoc/>
        public IOperationResource GetResourceById(Guid connectorId, OperationType dbAction, string dbObjectName)
        {
            var connector = _appAdminService.GetConnectionById(connectorId);
            return GetResourceType(connector, dbAction, dbObjectName);
        }


        protected virtual IOperationResource GetResourceType(Connector connector, OperationType dbAction, string dbObjectName)
        {
            Check.NotNull(connector, nameof(connector));

            IOperationResource resource;
            try
            {
                resource = _resources.Last(c => c.ResourceType == connector.ResourceType);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessages.DbTypeNotRegistered, connector.ResourceType), nameof(connector));
            }


            resource.ConfigureAction(connector, dbAction, dbObjectName);
            return resource;
        }
    }
}