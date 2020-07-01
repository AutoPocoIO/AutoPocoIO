using AutoPocoIO.DynamicSchema.Util;

namespace AutoPocoIO.Dashboard.ViewModels
{
    public class ConnectorViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? ResourceType { get; set; }
        public string Schema { get; set; }
        public string ConnectionStringDecrypted { get; private set; }
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
        public int? RecordLimit { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string InitialCatalog { get; set; }
        public string DataSource { get; set; }
        public int? Port { get; set; }
        public bool IsActive { get; set; }
    }
}
