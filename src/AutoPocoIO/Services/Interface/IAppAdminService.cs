using AutoPocoIO.Models;

namespace AutoPocoIO.Services
{
    public interface IAppAdminService
    {
        Connector GetConnectionById(string id);
        Connector GetConnection(string connectionName);
    }
}