using AutoPocoIO.Services;
using System.Linq;

namespace AutoPocoIO.Api
{
    /// <summary>
    /// Dynamicly access database views
    /// </summary>
    public interface IViewOperations
    {
        /// <summary>
        /// Retrieve data from a given view
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">Name of the view in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>Dyanamic IQueryable of the results</returns>
        IQueryable<object> GetAll(string connectorName, string viewName, ILoggingService loggingService = null);
        /// <summary>
        /// Retrieve data from a given view and project them to <typeparamref name="TViewModel"/>
        /// </summary>
        /// <typeparam name="TViewModel">Type to project the results to.</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">Name of the view in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>IQueryable of <typeparamref name="TViewModel"/></returns>
        IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string viewName, ILoggingService loggingService = null);
        /// <summary>
        /// Retrieve data from a given view
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="viewName">Name of the view in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>Dyanamic IQueryable of the results and the connector max.</returns>
        (IQueryable<object> list, int recordLimit) GetAllAndRecordLimit(string connectorName, string viewName, ILoggingService loggingService = null);
    }
}