using AutoPocoIO.Models;
using System;

namespace AutoPocoIO.Services
{
    public interface IAppAdminService
    {
        Connector GetConnectionById(Guid id);
        Connector GetConnection(string connectionName);
    }
}