using AutoPocoIO.Models;
using AutoPocoIO.Services;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AutoPocoIO.Api
{
    public interface ITableOperations
    {
        object CreateNewRow(string connectorName, string tableName, JToken value, ILoggingService loggingService = null);
        TViewModel CreateNewRow<TViewModel>(string connectorName, string tableName, TViewModel value, ILoggingService loggingService = null) where TViewModel : class;
        TableDefinition Definition(string connectorName, string tableName, ILoggingService loggingService = null);
        ColumnDefinition Definition(string connectorName, string tableName, string columnName, ILoggingService loggingService = null);
        object DeleteRow(string connectorName, string tableName, string id, ILoggingService loggingService = null);
        (IQueryable<object> list, int connectorMax) GetAll(string connectorName, string tableName, ILoggingService loggingService = null);
        IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string tableName, ILoggingService loggingService = null);
        object GetById(string connectorName, string tableName, string id, ILoggingService loggingService = null);
        TViewModel GetById<TViewModel>(string connectorName, string tableName, string id, ILoggingService loggingService = null) where TViewModel : class;
        object UpdateRow(string connectorName, string tableName, string keys, JToken value, ILoggingService loggingService = null);
        TViewModel UpdateRow<TViewModel>(string connectorName, string tableName, string id, TViewModel value, ILoggingService loggingService = null) where TViewModel : class;
    }
}