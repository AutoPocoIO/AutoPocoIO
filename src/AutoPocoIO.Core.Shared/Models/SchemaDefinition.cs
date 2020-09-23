using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoPocoIO.Models
{
    /// <summary>
    /// Information about the database schema
    /// </summary>
    public class SchemaDefinition
    {
        public SchemaDefinition()
        {
            Tables = new List<string>();
        }

        /// <summary>
        /// Database id for the connector
        /// </summary>
        [JsonProperty(Order = -2)]
        public string ConnectorId { get; set; }

        /// <summary>
        /// Alias of the connector
        /// </summary>
        [JsonProperty(Order = -2)]
        public string ConnectorName { get; set; }

        /// <summary>
        /// Schema name
        /// </summary>
        [JsonProperty(Order = -2)]
        public string Name { get; set; }

        /// <summary>
        /// Database name
        /// </summary>
        [JsonProperty(Order = -2)]
        public string DbName { get; set; }

        /// <summary>
        /// List of available table names
        /// </summary>

        /* Unmerged change from project 'AutoPocoIO (net461)'
        Before:
                [JsonProperty(Order = -2)]

                public IEnumerable<string> Tables { get; set; }
        After:
                [JsonProperty(Order = -2)]

                public IEnumerable<string> Tables { get; set; }
        */
        [JsonProperty(Order = -2)]

        public IEnumerable<string> Tables { get; set; }

        /// <summary>
        /// List of available view names
        /// </summary>
        [JsonProperty(Order = -2)]
        public IEnumerable<string> Views { get; set; }

        /// <summary>
        /// List of available stored procedure names
        /// </summary>
        [JsonProperty(Order = -2)]
        public IEnumerable<string> StoredProcedures { get; set; }


    }
}
