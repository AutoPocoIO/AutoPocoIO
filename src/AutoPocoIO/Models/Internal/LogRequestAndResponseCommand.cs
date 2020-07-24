using System;

namespace AutoPocoIO.Models
{
    public class LogRequestAndResponseCommand
    {
        public Guid RequestGuid { get; set; }
        public DateTime RequestTime { get; set; }

        public string Connector { get; set; }
        public string ResourceType { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }

        //Differences from MVC and WebApi
        public string RequestType { get; set; }

    }
}
