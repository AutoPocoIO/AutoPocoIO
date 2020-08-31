using AutoPocoIO.Constants;
using AutoPocoIO.Context;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPocoIO.Services
{
    /// <summary>
    /// Log AutoPoco commands
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        
        /// <summary>
        ///  Set up new logging for a request
        /// This constructor is not meant to be called in code.  Used for DI. 
        /// </summary>
        /// <param name="timeProvider">Localized time provider.</param>
        /// <param name="scopeFactory">Create to service scope for of thread logging.</param>
        public LoggingService(ITimeProvider timeProvider, IServiceScopeFactory scopeFactory, AutoPocoServiceOptions events)
        {
            _timeProvider = timeProvider;
            _serviceScopeFactory = scopeFactory;
            Events = events;
            ApiRequests = new List<LogRequestAndResponseCommand>();
        }


        /// <summary>
        /// Logging events setup in the service options
        /// </summary>
        protected AutoPocoServiceOptions Events { get; }

        /// <summary>
        /// List of request to be logged
        /// </summary>
        protected List<LogRequestAndResponseCommand> ApiRequests { get; }

        /// <inheritdoc/>
        public DateTime ResponseTime { get; set; }
        /// <inheritdoc/>
        public string StatusCode { get; set; }
        /// <inheritdoc/>
        public string Ip { get; set; }
        /// <inheritdoc/>
        public string Exception { get; set; }
        /// <inheritdoc/>
        public virtual int LogCount { get { return ApiRequests.Count; } }

        /// <inheritdoc/>
        public void AddTableToLogger(string connectorName, string tableName, HttpMethodType httpMethod)
        {
            AddApiRequest(new LoggingApiContextValues
            {
                ConnectorName = connectorName,
                ResourceName = tableName,
                ResourceType = "table",
                HttpMethod = httpMethod.ToString()
            }, _timeProvider.UtcNow);
        }

        /// <inheritdoc/>
        public void AddTableToLogger(string connectorName, string tableName, HttpMethodType httpMethod, object[] primaryKey)
        {
            AddApiRequest(new LoggingApiContextValues
            {
                ConnectorName = connectorName,
                ResourceName = tableName,
                ResourceType = "table",
                HttpMethod = httpMethod.ToString(),
                ResourceId = string.Join(";", primaryKey?.Select(c => c.ToString()) ?? new List<string>())
            }, _timeProvider.UtcNow);
        }

        /// <inheritdoc/>
        public void AddViewToLogger(string connectorName, string viewName)
        {
            AddApiRequest(new LoggingApiContextValues
            {
                ConnectorName = connectorName,
                ResourceName = viewName,
                ResourceType = "view",
                HttpMethod = HttpMethodType.GET.ToString()
            }, _timeProvider.UtcNow);
        }

        /// <inheritdoc/>
        public void AddSprocToLogger(string connectorName, string sprocName, HttpMethodType httpMethod)
        {
            AddApiRequest(new LoggingApiContextValues
            {
                ConnectorName = connectorName,
                ResourceName = sprocName,
                ResourceType = "sproc",
                HttpMethod = httpMethod.ToString()
            }, _timeProvider.UtcNow);
        }


        /// <inheritdoc/>
        public void AddSchemaToLogger(string connectorName)
        {
            AddApiRequest(new LoggingApiContextValues
            {
                ConnectorName = connectorName,
                ResourceName = "",
                ResourceType = "schema",
                HttpMethod = HttpMethodType.GET.ToString()
            }, _timeProvider.UtcNow);
        }


        /// <inheritdoc/>
        public Task LogAll()
        {
            var scope = _serviceScopeFactory.CreateScope();
            return Task.Run(() =>
            {
                try
                {
                    this.ApiRequests.ForEach(c => LogHttpRequestAndResponse(scope, c));
                }
                finally
                {
                    Events.OnLogged?.Invoke(scope.ServiceProvider, this);
                    scope.Dispose();
                }
            });
        }
        /// <inheritdoc/>
        public virtual void AddContextInfomation(ContextLogParameters logParameters)
        {
            Check.NotNull(logParameters, nameof(logParameters));

            ResponseTime = _timeProvider.UtcNow;
            StatusCode = logParameters.DescriptionFromStatusCode(logParameters.StatusCode);
            Ip = logParameters.GetIPFromLogParameters();
            Exception = logParameters.Exception;
        }

        protected virtual void LogHttpRequestAndResponse(IServiceScope scope, LogRequestAndResponseCommand command)
        {
            Check.NotNull(scope, nameof(scope));
            Check.NotNull(command, nameof(command));

            var provider = scope.ServiceProvider;
            Events.OnLogging?.Invoke(provider, command, this);

            var db = provider.GetRequiredService<LogDbContext>();
            LogHttpRequest(command, db);
            LogHttpResponse(command, db);
        }

        /// <summary>
        /// Appended an API request to be logged
        /// </summary>
        /// <param name="values">Request infomation</param>
        /// <param name="requestTime">Time of request</param>
        private void AddApiRequest(LoggingApiContextValues values, DateTime requestTime)
        {
            ApiRequests.Add(new LogRequestAndResponseCommand
            {
                Connector = values.ConnectorName,
                Resource = values.ResourceName,
                ResourceId = values.ResourceId,
                ResourceType = values.ResourceType,
                RequestType = values.HttpMethod,
                RequestTime = requestTime,
                RequestGuid = Guid.NewGuid()
            });
        }

        private void LogHttpRequest(LogRequestAndResponseCommand command, LogDbContext db)
        {
            RequestLog requestLog = new RequestLog
            {
                DateTimeUtc = command.RequestTime,
                RequesterIp = this.Ip,
                RequestGuid = command.RequestGuid,
                RequestId = command.RequestTime.Ticks,
                Connector = command.Connector,
                RequestType = command.RequestType,
                ResourceName = command.Resource,
                ResourceId = command.ResourceId
            };

            db.RequestLogs.Add(requestLog);
            db.SaveChanges();

        }

        private void LogHttpResponse(LogRequestAndResponseCommand command, LogDbContext db)
        {
            var responseLog = new ResponseLog
            {
                RequestGuid = command.RequestGuid,
                ResponseId = command.RequestTime.Ticks,
                DateTimeUtc = this.ResponseTime,
                Status = $"HTTP {this.StatusCode}",
                Exception = this.Exception
            };

            db.ResponseLogs.Add(responseLog);
            db.SaveChanges();
        }

    }
}