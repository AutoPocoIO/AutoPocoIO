using AutoPocoIO.Context;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using System;
using System.Linq;

namespace AutoPocoIO.Services
{
    internal class AppAdminService : IAppAdminService
    {
        private readonly AppDbContext _db;

        public AppAdminService(AppDbContext db)
        {
            _db = db;
        }

        public Connector GetConnection(string connectionName)
        {
            try
            {
                return _db.Connector.Single(c => c.Name == connectionName && c.IsActive);
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorNotFoundException(connectionName);
            }
        }

        public Connector GetConnection(int id)
        {
            try
            {
                return _db.Connector.Single(c => c.Id == id && c.IsActive);
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorNotFoundException(id);
            }
        }
    }
}