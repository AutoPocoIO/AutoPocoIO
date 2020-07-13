using AutoPocoIO.DynamicSchema.Util;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPocoIO.Models
{
    /// <summary>
    /// Connector details.
    /// </summary>
    [Table("Connector", Schema = "AutoPoco")]
    public class Connector
    {
        /// <summary>
        /// Initialize connector with unique identifier.
        /// </summary>
        public Connector()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Unique connector id.
        /// </summary>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// Friendly name for connector.
        /// </summary>
        [MaxLength(50)]
        public string Name { get; set; }
        /// <summary>
        /// Database type.
        /// </summary>
        public int ResourceType { get; set; }
        /// <summary>
        /// Database schmea.
        /// </summary>
        [MaxLength(50)]
        public string Schema { get; set; }
        /// <summary>
        /// Encrypted connection string.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Decrypted connection string.
        /// </summary>
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
        /// <summary>
        /// WebApi return limit.
        /// </summary>
        public int RecordLimit { get; set; }
        /// <summary>
        /// Database access user.
        /// </summary>
        [MaxLength(50)]
        public string UserId { get; set; }
        /// <summary>
        /// Database name.
        /// </summary>
        [MaxLength(50)]
        public string InitialCatalog { get; set; }
        /// <summary>
        /// Server path.
        /// </summary>
        [MaxLength(500)]
        public string DataSource { get; set; }
        /// <summary>
        /// Port number (if applicable).
        /// </summary>
        public int? Port { get; set; }
        /// <summary>
        /// Enable/Disable connector usage.
        /// </summary>
        public bool IsActive { get; set; }
    }
}