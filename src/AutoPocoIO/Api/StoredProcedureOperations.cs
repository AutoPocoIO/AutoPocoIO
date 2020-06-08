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
using System.Reflection;

namespace AutoPocoIO.Api
{
    /// <summary>
    /// Call Stored Procedures
    /// </summary>
    public class StoredProcedureOperations : IStoredProcedureOperations
    {
        private readonly IResourceFactory _resourceFactory;

        public StoredProcedureOperations(IServiceProvider serviceProvider)
        {
            _resourceFactory = serviceProvider.GetRequiredService<IResourceFactory>();
        }


        /// <summary>
        /// Execute Stored Procedure (No Params)
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Stored Procedure name in the database.</param>
        /// <param name="loggingService">Pass </param>
        /// <returns></returns>
        public IDictionary<string, object> ExecuteNoParameters(string connectorName, string procedureName, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, procedureName);
            return resource.ExecuteProc(new Dictionary<string, object>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Stored Procedure name in the database.</param>
        /// <param name="parameters"></param>
        /// <param name="loggingService"></param>
        /// <returns></returns>
        public IDictionary<string, object> Execute(string connectorName, string procedureName, JToken parameters, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.POST);
            Check.NotNull(parameters, nameof(parameters));

            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, procedureName);
            IDictionary<string, object> parameterDictionary = (IDictionary<string, object>)parameters.JTokenToConventionalDotNetObject();
            return resource.ExecuteProc(parameterDictionary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Stored Procedure name in the database.</param>
        /// <param name="parameters"></param>
        /// <param name="loggingService"></param>
        /// <returns></returns>
        public IDictionary<string, object> Execute<T>(string connectorName, string procedureName, T parameters, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.POST);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.read, procedureName);
            IDictionary<string, object> parameterDictionary = typeof(T)
                                                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                        .ToDictionary(prop => prop.Name, prop => prop.GetValue(parameters, null));
            return resource.ExecuteProc(parameterDictionary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Stored Procedure name in the database.</param>
        /// <param name="loggingService"></param>
        /// <returns></returns>
        public StoredProcedureDefinition Definition(string connectorName, string procedureName, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, procedureName);
            return resource.GetStoredProcedureDefinition();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectorName">Name of the database to access.</param>
        /// <param name="procedureName">Stored Procedure name in the database.</param>
        /// <param name="parameterName"></param>
        /// <param name="loggingService"></param>
        /// <returns></returns>
        public StoredProcedureParameterDefinition Definition(string connectorName, string procedureName, string parameterName, ILoggingService loggingService = null)
        {
            loggingService?.AddSprocToLogger(connectorName, procedureName, HttpMethodType.GET);
            IOperationResource resource = _resourceFactory.GetResource(connectorName, OperationType.Any, procedureName);
            return resource.GetStoredProcedureDefinition(parameterName);
        }
    }
}
