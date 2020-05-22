using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.AutoPoco;


namespace AutoPocoIO.Api
{
    /// <summary>
    /// Dynamicly access database tables
    /// </summary>
    public partial class TableOperations : ITableOperations
    {
        private readonly IResourceFactory _resourceFactory;
        private readonly IRequestQueryStringService _requestQuery;
        public TableOperations(IServiceProvider serviceProvider)
        {
            _resourceFactory = serviceProvider.GetRequiredService<IResourceFactory>();
            _requestQuery = serviceProvider.GetRequiredService<IRequestQueryStringService>();
        }


        /// <summary>
        /// Get all records from <paramref name="tableName"/>. Intended for WebAPI controller requests.
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>Dyanamic IQueryable of the results and the connector max.</returns>
        public (IQueryable<object> list, int connectorMax) GetAll(string connectorName, string tableName, ILoggingService loggingService = null)
        {
            Check.NotEmpty(connectorName, nameof(connectorName));
            Check.NotEmpty(tableName, nameof(tableName));

            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, tableName);

            var queryString = _requestQuery.GetQueryStrings();

            return (resource.GetResourceRecords(queryString), resource.Connector.RecordLimit);
        }

        /// <summary>
        /// Get all records from <paramref name="tableName"/> and projects the to a view model. Intended to be used as the initial part of a linq query.
        /// </summary>
        /// <typeparam name="TViewModel">Type to project the results to.</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>IQueryable of <typeparamref name="TViewModel"/></returns>
        public IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string tableName, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, tableName);

            //Manually $expand to prevent nulls on non pk joins
            var joinProperties = typeof(TViewModel).GetProperties()
                                       .Where(c => (c.PropertyType.IsClass && c.PropertyType != typeof(string)) ||
                                                            (c.PropertyType.IsGenericType && c.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                                       .Select(c => c.Name);

            IQueryable<dynamic> list;
            if (joinProperties.Any())
                list = resource.GetResourceRecords(new Dictionary<string, string>() { { "$expand", string.Join(",", joinProperties) } });
            else
                list = resource.GetResourceRecords(new Dictionary<string, string>());

            return list.ProjectTo<TViewModel>();
        }

        /// <summary>
        /// Retrieves a single record from a table by Primary Key. Note: for composite PKs,
        /// use a semicolon separated string.
        /// </summary>
        /// <param name="connectorName">The name of the connector to the table's schema.</param>
        /// <param name="tableName">The name of the table to retrieve the record from.</param>
        /// <param name="id">The primary key value of the record to be retrieved as a string. 
        /// For composite keys, use semicolon separated string</param>
        /// <param name="loggingService">LoggingService object to log the request. Null by default if no logging is required.</param>
        /// <returns>The record with matching PK. Null if not found.</returns>
        public object GetById(string connectorName, string tableName, string id, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, tableName);
            var data = resource.GetResourceRecordById(id);
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TViewModel">View Model Type</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id"></param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns></returns>
        public TViewModel GetById<TViewModel>(string connectorName, string tableName, string id, ILoggingService loggingService = null) where TViewModel : class
        {
            var obj = GetById(connectorName, tableName, id, loggingService);
            return DynamicObjectExtensions.PopulateModel<TViewModel>(obj);
        }

        /// <summary>
        /// Insert a recored into a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="value">JSON object to insert</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>An instance of the object inserted</returns>
        public object CreateNewRow(string connectorName, string tableName, JToken value, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.POST);
            Check.NotNull(value, nameof(value));

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.write, tableName);
            var data = resource.CreateNewResourceRecord(value);
            return data;
        }

        /// <summary>
        /// Insert a recored into a given table
        /// </summary>
        /// <typeparam name="TViewModel">Type of view model</typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="value">Object to insert into <paramref name="tableName"/></param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>An instance of the object inserted. </returns>
        public TViewModel CreateNewRow<TViewModel>(string connectorName, string tableName, TViewModel value, ILoggingService loggingService = null) where TViewModel : class
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.POST);

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.write, tableName);
            var data = resource.CreateNewResourceRecord(value);
            return DynamicObjectExtensions.PopulateModel<TViewModel>(data);
        }


        /// <summary>
        /// Update record in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="keys">Primary key(s)</param>
        /// <param name="value">JSON object to update</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>The updated object</returns>
        public object UpdateRow(string connectorName, string tableName, string keys, JToken value, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.PUT);
            Check.NotNull(value, nameof(value));

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.write, tableName);
            return resource.UpdateResourceRecordById(value, keys);
        }

        /// <summary>
        /// Update record in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id">Primary Key(s)</param>
        /// <param name="value">Object to updated in <paramref name="tableName"/></param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>The updated object</returns>
        public TViewModel UpdateRow<TViewModel>(string connectorName, string tableName, string id, TViewModel value, ILoggingService loggingService = null) where TViewModel : class
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.PUT);

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.write, tableName);
            return resource.UpdateResourceRecordById(value, id);
        }

        /// <summary>
        /// Remove record from a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="id">Primary Key(s)</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>The removed object</returns>
        public object DeleteRow(string connectorName, string tableName, string id, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.DELETE);

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.delete, tableName);
            return resource.DeleteResourceRecordById(id);
        }

        /// <summary>
        ///  Describes the table and includes list of columns that exists in a given table
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>A description of <paramref name="tableName"/></returns>
        public TableDefinition Definition(string connectorName, string tableName, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, tableName);
            return resource.GetTableDefinition();
        }

        /// <summary>
        /// View column attirbutes
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="tableName">Name of the table in the database.</param>
        /// <param name="columnName">Name of the column in the database.</param>
        /// <param name="loggingService">Include logging serivce to log this API call.</param>
        /// <returns>Column attributes</returns>
        public ColumnDefinition Definition(string connectorName, string tableName, string columnName, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, tableName);
            return resource.GetColumnDefinition(columnName);
        }


    }
}