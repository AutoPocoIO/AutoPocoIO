using AutoPocoIO.Models;

namespace AutoPocoIO.Services
{
    public interface IAppAdminService
    {
        Connector GetConnection(int id);
        Connector GetConnection(string connectionName);
    }
}