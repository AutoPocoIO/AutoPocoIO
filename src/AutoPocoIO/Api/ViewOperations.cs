using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Factories;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        public IOperationResource OperationResource { get; private set; }

        public ViewOperations(IServiceProvider serviceProvider)
        {
            _resourceFactory = serviceProvider.GetRequiredService<IResourceFactory>();
        }

        /// <summary>
        /// Retrieve data from a given view
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">Name of the view in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>Dyanamic IQueryable of the results and the connector max.</returns>
        public (IQueryable<object> list, int recordLimit) GetAllAndRecordLimit(string connectorName, string viewName, ILoggingService loggingService = null)
        {
            loggingService?.AddViewToLogger(connectorName, viewName);
            OperationResource = _resourceFactory.GetResource(connectorName, OperationType.read, viewName);
            return (OperationResource.GetViewRecords(), OperationResource.Connector.RecordLimit);
        }

        /// <summary>
        /// Retrieve data from a given view
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">Name of the view in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>Dyanamic IQueryable of the results</returns>
        public IQueryable<object> GetAll(string connectorName, string viewName, ILoggingService loggingService = null)
        {
            loggingService?.AddViewToLogger(connectorName, viewName);
            OperationResource = _resourceFactory.GetResource(connectorName, OperationType.read, viewName);
            return OperationResource.GetViewRecords();
        }

        /// <summary>
        /// Retrieve data from a given view and project them to <typeparamref name="TViewModel"/>
        /// </summary>
        /// <typeparam name="TViewModel">Type to project the results to.</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">Name of the view in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>IQueryable of <typeparamref name="TViewModel"/></returns>

        public IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string viewName, ILoggingService loggingService = null)
        {
            return GetAll(connectorName, viewName, loggingService)
                     .ProjectTo<TViewModel>();
        }
    }
}
