using AutoPocoIO.DynamicSchema.Util;
using System;

namespace AutoPocoIO.Dashboard.ViewModels
{
    /// <summary>
    /// Connector details.
    /// </summary>
    public class ConnectorViewModel
    {
        /// <summary>
        /// Unique connector id.
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Friendly name for connector.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Database type.
        /// </summary>
        public string ResourceType { get; set; }
        /// <summary>
        /// Database schmea.
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Decrypted connection string.
        /// </summary>
        public string ConnectionStringDecrypted { get; private set; }
        /// <summary>
        /// Encrypted connection string.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return EncryptDecrypt.EncryptString(ConnectionStringDecrypted);
            }

            set
            {
                ConnectionStringDecrypted = EncryptDecrypt.DecryptString(value);
            }
        }
        /// <summary>
        /// WebApi return limit.
        /// </summary>
        public int? RecordLimit { get; set; }
        /// <summary>
        /// Database access user.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Database access password.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Database name.
        /// </summary>
        public string InitialCatalog { get; set; }
        /// <summary>
        /// Server path.
        /// </summary>
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
