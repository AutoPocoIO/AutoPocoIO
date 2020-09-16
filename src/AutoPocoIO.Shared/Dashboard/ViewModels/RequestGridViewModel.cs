using System;

namespace AutoPocoIO.Dashboard.ViewModels
{
    /// <summary>
    /// Display information about recent requests.
    /// </summary>
    public class RequestGridViewModel
    {
        /// <summary>
        /// Unique identifier for request.
        /// </summary>
        public Guid? RequestGuid { get; set; }
        /// <summary>
        /// Time of request in GMT
        /// </summary>
        public DateTime? DateTimeUtc { get; set; }
        /// <summary>
        /// Http request method
        /// </summary>
        public string RequestType { get; set; }
        /// <summary>
        /// Name of used connector
        /// </summary>
        public string Connector { get; set; }
        /// <summary>
        /// Http response status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Who is making the request
        /// </summary>
        public string Requester { get; set; }
        /// <summary>
        /// Database object name
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// Database Primary key (if applicable)
        /// </summary>
        public string ResourceId { get; set; }

    }
}
