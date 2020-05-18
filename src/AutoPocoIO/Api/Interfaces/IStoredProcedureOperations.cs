using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Newtonsoft.Json.Linq;

namespace AutoPocoIO.Api
{
    public interface IStoredProcedureOperations
    {
        StoredProcedureDefinition Definition(string connectorName, string procedureName, ILoggingService loggingService = null);
        StoredProcedureParameterDefinition Definition(string connectorName, string procedureName, string parameterName, ILoggingService loggingService = null);
        object ExecuteNoParameters(string connectorName, string procedureName, ILoggingService loggingService = null);
        object Execute(string connectorName, string procedureName, JToken parameters, ILoggingService loggingService = null);
        object Execute<T>(string connectorName, string procedureName, T parameters, ILoggingService loggingService = null);
    }
}