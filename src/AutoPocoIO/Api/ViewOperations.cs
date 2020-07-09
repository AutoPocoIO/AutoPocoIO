using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using System.Linq;
using System.Linq.AutoPoco;

namespace AutoPocoIO.Api
{
    /// <summary>
    /// Dynamicly access database views
    /// </summary>
    public class ViewOperations : IViewOperations
    {
        private readonly IResourceFactory _resourceFactory;

        /// <summary>
        /// Initialize view operations with access to all registered resource types.
        /// </summary>
        /// <param name="resourceFactory">Get resource from the connector.</param>
        public ViewOperations(IResourceFactory resourceFactory)
        {
            _resourceFactory = resourceFactory;
        }

        /// <inheritdoc />
        public (IQueryable<object> list, int recordLimit) GetAllAndRecordLimit(string connectorName, string viewName, ILoggingService loggingService = null)
        {
            loggingService?.AddViewToLogger(connectorName, viewName);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, viewName);
            return (resource.GetViewRecords(), resource.Connector.RecordLimit);
        }

        /// <inheritdoc />
        public IQueryable<object> GetAll(string connectorName, string viewName, ILoggingService loggingService = null)
        {
            loggingService?.AddViewToLogger(connectorName, viewName);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, viewName);
            return resource.GetViewRecords();
        }


        /// <inheritdoc />
        public IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string viewName, ILoggingService loggingService = null)
        {
            return GetAll(connectorName, viewName, loggingService)
                     .ProjectTo<TViewModel>();
        }
    }
}
