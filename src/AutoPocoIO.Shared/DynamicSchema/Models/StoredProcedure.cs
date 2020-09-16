using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Models
{
    /// <summary>
    /// Stored procedure details
    /// </summary>
    public class StoredProcedure
    {
        /// <summary>
        /// Initialize parameter list.
        /// </summary>
        public StoredProcedure()
        {
            Parameters = new List<DBParameter>();
        }
        /// <summary>
        /// Procedure name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of parameters.
        /// </summary>
        public List<DBParameter> Parameters { get; }
        /// <summary>
        /// Database schema
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Database
        /// </summary>
        public string Database { get; set; }
    }
}
