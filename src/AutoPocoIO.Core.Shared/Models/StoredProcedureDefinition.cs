using System.Collections.Generic;

namespace AutoPocoIO.Models
{
    /// <summary>
    /// Information about a given stored procedure
    /// </summary>
    public class StoredProcedureDefinition
    {
        /// <summary>
        /// Store procedure name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of stored procedure parameter definitions
        /// </summary>
        public IEnumerable<StoredProcedureParameterDefinition> Parameters { get; set; }
    }
}
