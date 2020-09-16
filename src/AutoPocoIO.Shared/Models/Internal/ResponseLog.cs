using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPocoIO.Models
{
    [Table("Response", Schema = "AutoPocoLog")]
    internal class ResponseLog
    {
        public long ResponseId { get; set; }

        public Guid RequestGuid { get; set; }

        [Column(TypeName = "datetime2(4)")]
        public DateTime? DateTimeUtc { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? DayOfResponse { get; set; }

        [MaxLength(51)]
        public string Status { get; set; }

        public string Exception { get; set; }


    }
}
