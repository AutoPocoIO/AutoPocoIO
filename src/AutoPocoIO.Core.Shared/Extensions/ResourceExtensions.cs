using AutoPocoIO.Context;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Models;
using AutoPocoIO.Resources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.AutoPoco;

namespace AutoPocoIO.Extensions
{
    /// <summary>
    /// Additional methods for database objects.
    /// </summary>
    public static class ResourceExtensions
    {
        /// <summary>
        /// Maps a result set to a view model
        /// </summary>
        /// <typeparam name="TViewModel">View Model type</typeparam>
        /// <param name="outputParameters">Output from the stored procedure execution.</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <exception cref="ArgumentException">Thrown when the parameter is not found or not a result set.</exception>
        /// <returns></returns>
        public static IEnumerable<TViewModel> ProjectResultSet<TViewModel>(this IDictionary<string, object> outputParameters, string parameterName)
        {
            Check.NotNull(outputParameters, nameof(outputParameters));
            Check.NotNull(parameterName, nameof(parameterName));
            if (outputParameters.ContainsKey(parameterName))
            {
                if (outputParameters[parameterName] is IEnumerable<object>)
                {
                    var results = (IEnumerable<IDictionary<string, object>>)outputParameters[parameterName];
                    return results.ProjectResultTo<TViewModel>();
                }
                else
                    throw new ArgumentException($"Output Parameter {parameterName} is not a result set.  It is type {outputParameters[parameterName].GetType()}.", nameof(parameterName));
            }
            else
            {
                throw new ArgumentException($"The parameter {parameterName} was not found.  The following were found: {string.Join(",", outputParameters.Keys)}.", nameof(parameterName));
            }
        }

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

        internal static void SetConnectionInfo(this Connector connector, ConnectionInformation connectionInformation)
        {
            connector.InitialCatalog = connectionInformation.InitialCatalog;
            connector.UserId = connectionInformation.UserId;
            connector.DataSource = connectionInformation.DataSource;
            connector.ResourceType = connectionInformation.ResourceType;
        }


    }
}
