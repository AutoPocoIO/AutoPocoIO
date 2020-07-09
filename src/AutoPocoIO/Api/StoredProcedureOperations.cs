using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Factories;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Api
{

    /// <summary>
    ///  API for accessings stored procedures.
    /// </summary>
    public class StoredProcedureOperations : IStoredProcedureOperations
    {
        private readonly IResourceFactory _resourceFactory;

        /// <summary>
        /// Initialize store procedure operations with access to all registered resource types.
        /// </summary>
        /// <param name="resourceFactory">Get resource from the connector</param>
        public StoredProcedureOperations(IResourceFactory resourceFactory)
        {
            _resourceFactory = resourceFactory;
        }


        /// <inheritdoc />
        public IDictionary<string, object> ExecuteNoParameters(string connectorName, string procedureName, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, procedureName);
            return resource.ExecuteProc(new Dictionary<string, object>());
        }

        /// <inheritdoc />
        public IDictionary<string, object> Execute(string connectorName, string procedureName, JToken parameters, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.POST);
            Check.NotNull(parameters, nameof(parameters));

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, procedureName);
            IDictionary<string, object> parameterDictionary = (IDictionary<string, object>)parameters.JTokenToConventionalDotNetObject();
            return resource.ExecuteProc(parameterDictionary);
        }

        /// <inheritdoc />
        public IDictionary<string, object> Execute<TViewModel>(string connectorName, string procedureName, TViewModel parameters, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.POST);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, procedureName);
            IDictionary<string, object> parameterDictionary = typeof(TViewModel)
                                                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                        .ToDictionary(prop => prop.Name, prop => prop.GetValue(parameters, null));
            return resource.ExecuteProc(parameterDictionary);
        }

        /// <inheritdoc />
        public StoredProcedureDefinition Definition(string connectorName, string procedureName, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, procedureName);
            return resource.GetStoredProcedureDefinition();
        }

        /// <inheritdoc />
        public StoredProcedureParameterDefinition Definition(string connectorName, string procedureName, string parameterName, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, procedureName);
            return resource.GetStoredProcedureDefinition(parameterName);
        }
    }
}
