﻿using System;

namespace AutoPocoIO.Dashboard.ViewModels
{
    public class RequestGridViewModel
    {
        public Guid? RequestGuid { get; set; }
        public DateTime? DateTimeUtc { get; set; }
        public string RequestType { get; set; }
        public string Connector { get; set; }
        public string Status { get; set; }
        public string Requester { get; set; }
        public string Resource { get; set; }
    }
}