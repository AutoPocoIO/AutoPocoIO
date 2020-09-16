using AutoPocoIO.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AutoPocoIO.DynamicSchema.Models
{
    /// <summary>
    /// Configuration settings for dynamically pulling DB objects
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Config()
        {
            PropertyPreFixName = "";
            UserDefinedJoins = new List<UserJoinConfiguration>();
        }

        /// <summary>
        /// Target schema.
        /// </summary>
        public string FilterSchema { get; set; }
        /// <summary>
        /// Target table.
        /// </summary>
        public string IncludedTable { get; set; }
        /// <summary>
        /// Target stored procedure.
        /// </summary>
        public string IncludedStoredProcedure { get; set; }

        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Additional column prefix
        /// </summary>
        public string PropertyPreFixName { get; set; }
        /// <summary>
        /// User created joins
        /// </summary>
        internal IEnumerable<UserJoinConfiguration> UserDefinedJoins { get; set; }
        /// <summary>
        /// Connector name.
        /// </summary>
        public string DatabaseConnectorName { get; set; }

        /// <summary>
        /// Formated user joins and virtual entity joins for schema call.
        /// </summary>
        public string JoinsAsString
        {
            get
            {
                if (UserDefinedJoins.Any())
                {
                    var joins = UserDefinedJoins.Select(x => string.Format(CultureInfo.InvariantCulture, "Object_ID('{0}.{1}')", x.PrincipalSchema, x.PrincipalTable))
                                           .Union(UserDefinedJoins.Select(x => string.Format(CultureInfo.InvariantCulture, "Object_ID('{0}.{1}')", x.DependentSchema, x.DependentTable)))
                                           .Where(x => x != string.Format(CultureInfo.InvariantCulture, "Object_ID('{0}.{1}')", FilterSchema, IncludedTable));

                    return joins.Any() ? string.Join(",", joins) : "''";
                }
                return "''";
            }
        }

        /// <summary>
        /// All connectors used to call database object.  Used to link cross database calls.
        /// </summary>
        public IEnumerable<string> UsedConnectors { get; set; }
    }
}
