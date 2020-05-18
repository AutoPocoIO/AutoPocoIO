using AutoPocoIO.DynamicSchema.Util;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPocoIO.Models
{
    [Table("Connector", Schema = "AutoPoco")]
    public class Connector
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public int ResourceType { get; set; }
        [MaxLength(50)]
        public string Schema { get; set; }
        public string ConnectionString { get; set; }
        [NotMapped]
        public string ConnectionStringDecrypted
        {
            get
            {
                return EncryptDecrypt.DecryptString(this.ConnectionString);
            }
            set
            {
                ConnectionString = EncryptDecrypt.EncryptString(value);
            }
        }
        public int RecordLimit { get; set; }
        [MaxLength(50)]
        public string UserId { get; set; }
        [MaxLength(50)]
        public string InitialCatalog { get; set; }
        [MaxLength(500)]
        public string DataSource { get; set; }
        public int? Port { get; set; }
    }
}