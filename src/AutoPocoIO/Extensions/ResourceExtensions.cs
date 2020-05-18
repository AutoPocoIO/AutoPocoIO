using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.Extensions
{
    internal static class ResourceExtensions
    {
        internal static void LoadUserDefinedTables(this Config config, Connector connector, AppDbContext appDb)
        {
            var joins = appDb.UserJoin.Where(c => c.PKConnectorId == connector.Id || c.FKConnectorId == connector.Id)
                 .Where(c => config.IncludedTable == c.PKTableName ||
                             config.IncludedTable == c.FKTableName)
                 .Include(c => c.PKConnector)
                 .Include(c => c.FKConnector);

            config.UsedConnectors = new List<string> { connector.ConnectionStringDecrypted };

            config.UsedConnectors = config.UsedConnectors.Union(joins.Select(c => c.PKConnector.ConnectionStringDecrypted))
                .Union(joins.Select(c => c.FKConnector.ConnectionStringDecrypted))
                .Distinct();

            config.UserDefinedJoins = joins.Select(c => new UserJoinConfiguration
            {
                Alias = c.Alias,
                PrincipalDatabase = c.PKConnector.InitialCatalog,
                PrincipalSchema = c.PKConnector.Schema,
                PrincipalTable = c.PKTableName,
                PrincipalColumns = c.PKColumn,
                DependentDatabase = c.FKConnector.InitialCatalog,
                DependentSchema = c.FKConnector.Schema,
                DependentTable = c.FKTableName,
                DependentColumns = c.FKColumn
            });
        }

        public static void SetConnectionInfo(this Connector connector, ConnectionInformation connectionInformation)
        {
            connector.InitialCatalog = connectionInformation.InitialCatalog;
            connector.UserId = connectionInformation.UserId;
            connector.DataSource = connectionInformation.DataSource;
        }
    }
}
