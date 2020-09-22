using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.AutoPoco;
using System.Linq.Dynamic.Core;


namespace AutoPocoIO.Api
{
    /// <summary>
    /// Dynamicly access database tables
    /// </summary>
    public partial class TableOperations : ITableOperations
    {
        
        private readonly IRequestQueryStringService _requestQuery;
        /// <summary>
        /// Initialize table operations with access to all registered resource types.
        /// </summary>
        /// <param name="resourceFactory">Get resource from the connector.</param>
        /// <param name="requestQuery">Get query string information for odata operations.</param>
        public TableOperations(IResourceFactory resourceFactory, IRequestQueryStringService requestQuery)
        {
            ResourceFactory = resourceFactory;
            _requestQuery = requestQuery;
        }

        /// <inheritdoc />
        public IResourceFactory ResourceFactory { get; }

        /// <inheritdoc />
        public (IQueryable<object> list, int connectorMax) GetAll(string connectorName, string tableName, ILoggingService loggingService = null)
        {
            Check.NotEmpty(connectorName, nameof(connectorName));
            Check.NotEmpty(tableName, nameof(tableName));

            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.read, tableName);

            var queryString = _requestQuery.GetQueryStrings();

            return (resource.GetResourceRecords(queryString), resource.Connector.RecordLimit);
        }

        /// <inheritdoc />
        public IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string tableName, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.read, tableName);

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

        /// <inheritdoc />
        public object GetById(string connectorName, string tableName, ILoggingService loggingService, params object[] id)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET, id);

            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.read, tableName);
            var data = resource.GetResourceRecordById(id);
            return data;
        }

        /// <inheritdoc />
        public object GetById(string connectorName, string tableName, params object[] id)
        {
            return GetById(connectorName, tableName, null, id);
        }

        /// <inheritdoc />
        public TViewModel GetById<TViewModel>(string connectorName, string tableName, ILoggingService loggingService, params object[] id) where TViewModel : class
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET, id);

            //Manually $expand to prevent nulls on non pk joins
            var joinProperties = typeof(TViewModel).GetProperties()
                                       .Where(c => (c.PropertyType.IsClass && c.PropertyType != typeof(string)) ||
                                                            (c.PropertyType.IsGenericType && c.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                                       .Select(c => c.Name);

            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.read, tableName);

            if (joinProperties.Any())
                return resource.GetResourceRecordById<TViewModel>(id, new Dictionary<string, string>() { { "$expand", string.Join(",", joinProperties) } });
            else
                return resource.GetResourceRecordById<TViewModel>(id, new Dictionary<string, string>());
        }

        /// <inheritdoc />
        public TViewModel GetById<TViewModel>(string connectorName, string tableName,  params object[] id) where TViewModel : class
        {
            return GetById<TViewModel>(connectorName, tableName, null, id);
        }

        /// <inheritdoc />
        public object CreateNewRow(string connectorName, string tableName, JToken value, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.POST);
            Check.NotNull(value, nameof(value));

            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.write, tableName);
            var data = resource.CreateNewResourceRecord(value);
            return data;
        }

        /// <inheritdoc />
        public TViewModel CreateNewRow<TViewModel>(string connectorName, string tableName, TViewModel value, ILoggingService loggingService = null) where TViewModel : class
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.POST);

            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.write, tableName);
            var data = resource.CreateNewResourceRecord(value);
            return DynamicObjectExtensions.PopulateModel<TViewModel>(data);
        }


        /// <inheritdoc />
        public object UpdateRow(string connectorName, string tableName, JToken value, ILoggingService loggingService = null)
        {
            Check.NotNull(value, nameof(value));

            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.write, tableName);
            object[] id = null;
            try
            {
                id = resource.GetPrimaryKeys(value);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //Log even if values for PK not found
                loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.PUT, id);
            }
            return resource.UpdateResourceRecordById(value, id);
        }

        /// <inheritdoc />
        public TViewModel UpdateRow<TViewModel>(string connectorName, string tableName, TViewModel value, ILoggingService loggingService = null) where TViewModel : class
        {
            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.write, tableName);

            object[] id = null;
            try
            {
                id = resource.GetPrimaryKeys(value);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //Log even if values for PK not found
                loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.PUT, id);
            }
            return resource.UpdateResourceRecordById(value, id);
        }

        /// <inheritdoc />
        public object DeleteRow(string connectorName, string tableName, ILoggingService loggingService, params object[] id)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.DELETE, id);

            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.delete, tableName);
            return resource.DeleteResourceRecordById(id);
        }

        /// <inheritdoc />
        public object DeleteRow(string connectorName, string tableName, params object[] id)
        {
            return DeleteRow(connectorName, tableName, null, id);
        }

        /// <inheritdoc />
        public TableDefinition Definition(string connectorName, string tableName, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.Any, tableName);
            return resource.GetTableDefinition();
        }

        /// <inheritdoc />
        public ColumnDefinition Definition(string connectorName, string tableName, string columnName, ILoggingService loggingService = null)
        {
            loggingService?.AddTableToLogger(connectorName, tableName, HttpMethodType.GET);
            IOperationResource resource = ResourceFactory.GetResource(connectorName, OperationType.Any, tableName);
            return resource.GetColumnDefinition(columnName);
        }
    }
}