using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AutoPocoIO.Factories
{
    public class ResourceFactory : IResourceFactory
    {
        private readonly IAppAdminService _appAdminService;
        private readonly IEnumerable<IOperationResource> _resources;

        public ResourceFactory(IAppAdminService appAdminService, IEnumerable<IOperationResource> resources)
        {
            _resources = resources;
            _appAdminService = appAdminService;
        }


        public IOperationResource GetResource(string connectorName, OperationType dbAction, string dbObjectName)
        {
            var connector = _appAdminService.GetConnection(connectorName);
            return GetResourceType(connector, dbAction, dbObjectName);
        }

        public IOperationResource GetResource(int connectorId, string dbObjectName)
        {
            var connector = _appAdminService.GetConnection(connectorId);
            return GetResourceType(connector, OperationType.read, dbObjectName);
        }

        public IOperationResource GetResource(int connectorId, OperationType dbAction, string dbObjectName)
        {
            var connector = _appAdminService.GetConnection(connectorId);
            return GetResourceType(connector, dbAction, dbObjectName);
        }

        private IOperationResource GetResourceType(Connector connector, OperationType dbAction, string dbObjectName)
        {
            IOperationResource resource;

            if (Enum.IsDefined(typeof(ResourceType), connector.ResourceType))
            {
                ResourceType resourceType = (ResourceType)connector.ResourceType;

                try
                {
                    resource = _resources.First(c => c.ResourceType == resourceType);
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessages.DbTypeNotRegistered, resourceType), nameof(connector));
                }
            }
            else
                throw new ArgumentOutOfRangeException(ExceptionMessages.DbAdapterNotFound);


            resource.ConfigureAction(connector, dbAction, dbObjectName);
            return resource;
        }
    }
}