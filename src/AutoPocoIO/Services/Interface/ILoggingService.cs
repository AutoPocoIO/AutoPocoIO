using AutoPocoIO.Constants;
using AutoPocoIO.LoggingMiddleware;
using System;
using System.Threading.Tasks;

namespace AutoPocoIO.Services
{
    public interface ILoggingService
    {
        int LogCount { get; }

        string Exception { get; set; }
        string StatusCode { get; set; }
        DateTime ResponseTime { get; set; }
        string Ip { get; set; }


        void AddSchemaToLogger(string connectorName);
        void AddSprocToLogger(string connectorName, string sprocName, HttpMethodType httpMethod);
        void AddTableToLogger(string connectorName, string tableName, HttpMethodType httpMethod);
        void AddViewToLogger(string connectorName, string viewName);
        Task LogAll();
        void AddContextInfomation(ContextLogParameters logParameters);
    }
}