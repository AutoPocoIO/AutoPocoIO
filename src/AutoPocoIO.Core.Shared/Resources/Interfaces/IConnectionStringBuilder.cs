using AutoPocoIO.DynamicSchema.Enums;

namespace AutoPocoIO.Resources
{
    public interface IConnectionStringBuilder
    {
        /// <summary>
        /// Entity framework provider type.
        /// </summary>
        string ResourceType { get; }

        ConnectionInformation ParseConnectionString(string connectionString);
        string CreateConnectionString(ConnectionInformation connectionInformation);
    }
}
