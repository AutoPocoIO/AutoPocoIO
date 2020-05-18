using AutoPocoIO.Models;
using AutoPocoIO.Services;

namespace AutoPocoIO.Api
{
    public interface ISchemaOperations
    {
        SchemaDefinition Definition(string connectorName, ILoggingService loggingService = null);
    }
}