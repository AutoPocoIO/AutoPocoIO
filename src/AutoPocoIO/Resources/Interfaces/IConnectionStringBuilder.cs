using AutoPocoIO.DynamicSchema.Enums;

namespace AutoPocoIO.Resources
{
    public interface IConnectionStringBuilder
    {
        ResourceType ResourceType { get; }

        ConnectionInformation ParseConnectionString(string connectionString);
        string CreateConnectionString(ConnectionInformation connectionInformation);
    }
}
