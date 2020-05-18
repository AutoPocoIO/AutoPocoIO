using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPocoIO.Models
{
    [Table("Request", Schema = "AutoPocoLog")]
    internal class RequestLog
    {
        public long RequestId { get; set; }
        public Guid RequestGuid { get; set; }

        [MaxLength(39)]
        public string RequesterIp { get; set; }

        [Column(TypeName = "datetime2(4)")]
        public DateTime? DateTimeUtc { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? DayOfRequest { get; set; }

        [MaxLength(10)]
        public string RequestType { get; set; }

        [MaxLength(50)]
        public string Connector { get; set; }
    }
}
