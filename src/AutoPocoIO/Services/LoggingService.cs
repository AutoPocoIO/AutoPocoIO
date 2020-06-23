﻿using AutoPocoIO.Constants;
using AutoPocoIO.Context;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.LoggingMiddleware;
using AutoPocoIO.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        public LoggingService(ITimeProvider timeProvider, IServiceScopeFactory scopeFactory)
        {
            _timeProvider = timeProvider;
            _serviceScopeFactory = scopeFactory;
            ApiRequests = new List<LogRequestAndResponseCommand>();
        }

        protected List<LogRequestAndResponseCommand> ApiRequests { get; }

        public DateTime ResponseTime { get; set; }
        public string StatusCode { get; set; }
        public string Ip { get; set; }
        public string Exception { get; set; }
        /// <summary>
        /// Check number of API request to be logged for a request
        /// </summary>
        public virtual int LogCount { get { return ApiRequests.Count; } }

        /// <summary>
        ///  Represents an event called for each api request
        /// </summary>
        public Action<IServiceProvider, LogRequestAndResponseCommand, ILoggingService> OnLogging { get; set; }
        /// <summary>
        ///  Represents an event called after the http request is logged 
        /// </summary>
        public Action<IServiceProvider, ILoggingService> OnLogged { get; set; }



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
                ResourceType = values.ResourceType,
                RequestType = values.HttpMethod,
                RequestTime = requestTime,
                RequestGuid = Guid.NewGuid(),
            });
        }

        /// <summary>
        /// Appened a table request
        /// </summary>
        /// <param name="connectorName">AutoPoco Connector name</param>
        /// <param name="tableName">Table accessed</param>
        /// <param name="httpMethod">Requst type (GET, POST, PUT, DELETE)</param>
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

        /// <summary>
        /// Append a view request
        /// </summary>
        /// <param name="connectorName">AutoPoco connector name</param>
        /// <param name="viewName">View accessed</param>
        /// 
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

        /// <summary>
        /// Append a stored procedure request
        /// </summary>
        /// <param name="connectorName">AutoPoco connector name</param>
        /// <param name="sprocName">Stored Procedure name</param>
        /// <param name="httpMethod">Requst type (GET, POST, PUT, DELETE)</param>
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


        /// <summary>
        /// Append a schema request
        /// </summary>
        /// <param name="connectorName">AutoPoco connector name</param>
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


        /// <summary>
        /// Log all pendeing API request
        /// </summary>
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
                    OnLogged?.Invoke(scope.ServiceProvider, this);
                    scope.Dispose();
                }
            });
        }


        protected virtual void LogHttpRequestAndResponse(IServiceScope scope, LogRequestAndResponseCommand command)
        {
            Check.NotNull(scope, nameof(scope));
            Check.NotNull(command, nameof(command));

            var provider = scope.ServiceProvider;
            OnLogging?.Invoke(provider, command, this);

            var db = provider.GetRequiredService<LogDbContext>();
            LogHttpRequest(command, db);
            LogHttpResponse(command, db);
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
                RequestType = command.RequestType
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

        public virtual void AddContextInfomation(ContextLogParameters logParameters)
        {
             Check.NotNull(logParameters, nameof(logParameters));
            
            ResponseTime = _timeProvider.UtcNow;
            StatusCode = logParameters.DescriptionFromStatusCode(logParameters.StatusCode);
            Ip = logParameters.GetIPFromLogParameters();
            Exception = logParameters.Exception;
        }
    }
}