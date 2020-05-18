using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPocoIO.Models
{
    [Table("UserJoin", Schema = "AutoPoco")]
    public class UserJoin 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string Alias { get; set; }
        [ForeignKey("PKConnector")]
        public int? PKConnectorId { get; set; }
        [ForeignKey("FKConnector")]
        public int? FKConnectorId { get; set; }
        [MaxLength(100)]
        [Required]
        public string PKTableName { get; set; }
        [MaxLength(100)]
        [Required]
        public string FKTableName { get; set; }
        [MaxLength(500)]
        [Required]
        public string PKColumn { get; set; }
        [MaxLength(500)]
        [Required]
        public string FKColumn { get; set; }

        public virtual Connector PKConnector { get; set; }
        public virtual Connector FKConnector { get; set; }
    }
}