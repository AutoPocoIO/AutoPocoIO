﻿using AutoPocoIO.Context;
using AutoPocoIO.Dashboard.ViewModels;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.Dashboard.Repos
{
    /// <summary>
    ///  Get information about request logs.
    /// </summary>
    public class RequestHistoryRepo : IRequestHistoryRepo
    {
        private readonly LogDbContext _db;
        private readonly ITimeProvider _timeProvider;

        /// <summary>
        /// Initialize repository.
        /// </summary>
        /// <param name="provider">Middleware scoped provider.</param>
        /// <param name="timeProvider">Server time information.</param>
        public RequestHistoryRepo(IServiceProvider provider, ITimeProvider timeProvider)
        {
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(timeProvider, nameof(timeProvider));

            _db = provider.GetService<LogDbContext>();
            _timeProvider = timeProvider;
        }

        ///<inheritdoc/>
        public virtual IEnumerable<RequestGridViewModel> ListRequest(int recordLimit)
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(_timeProvider.Now).TotalMinutes;

            var requests = _db.RequestLogs.OrderByDescending(c => c.DateTimeUtc)
                                       .Take(recordLimit)
                                       .GroupJoin(_db.ResponseLogs, c => new { c.RequestId, c.RequestGuid }, c => new { RequestId = c.ResponseId, c.RequestGuid },
                                               (req, resp) => new { Request = req, resp.FirstOrDefault().Status });

            return requests.Select(c => new RequestGridViewModel
            {
                Connector = c.Request.Connector,
                Requester = c.Request.RequesterIp,
                RequestGuid = c.Request.RequestGuid,
                RequestType = c.Request.RequestType,
                Resource = c.Request.ResourceName,
                ResourceId = c.Request.ResourceId,
                Status = c.Status,
                DateTimeUtc = c.Request.DateTimeUtc == null ? (DateTime?)null :
                                             c.Request.DateTimeUtc.Value.AddMinutes(offset)
            });
        }
    }
}
