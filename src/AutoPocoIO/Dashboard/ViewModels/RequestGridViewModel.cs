using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
