namespace AutoPocoIO.Models
{
    /// <summary>
    /// Properties of a stored procedure parameter
    /// </summary>
    public class StoredProcedureParameterDefinition
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Database type of the parameter value
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Is the parameter an output of the stored procedure
        /// </summary>
        public bool IsOutput { get; set; }

        /// <summary>
        /// Does the procedure except null as a value
        /// </summary>
        public bool IsNullable { get; set; }
    }
}
