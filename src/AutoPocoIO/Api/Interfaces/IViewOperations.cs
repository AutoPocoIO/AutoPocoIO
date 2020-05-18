using AutoPocoIO.Resources;
using AutoPocoIO.Services;
using System.Linq;

namespace AutoPocoIO.Api
{
    public interface IViewOperations
    {
        IOperationResource OperationResource { get; }
        IQueryable<object> GetAll(string connectorName, string viewName, ILoggingService loggingService = null);
        IQueryable<TViewModel> GetAll<TViewModel>(string connectorName, string viewName, ILoggingService loggingService = null);
        (IQueryable<object> list, int recordLimit) GetAllAndRecordLimit(string connectorName, string viewName, ILoggingService loggingService = null);
    }
}