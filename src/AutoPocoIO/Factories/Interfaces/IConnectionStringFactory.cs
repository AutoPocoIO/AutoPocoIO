using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AutoPocoIO.Factories
{
    public interface IConnectionStringFactory
    {
        string CreateConnectionString(DatabaseFacade database, ConnectionInformation connectionInformation);
        string CreateConnectionString(int resourceType, ConnectionInformation connectionInformation);
        ConnectionInformation GetConnectionInformation(DatabaseFacade database);
        ConnectionInformation GetConnectionInformation(int resourceType, string connectionString);
    }
}