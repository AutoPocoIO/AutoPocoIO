namespace AutoPocoIO.DynamicSchema.Models
{
    /// <summary>
    /// Stored procedure details.
    /// </summary>
    public class DBParameter
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Database type.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Flag if the parameter is Out or InOut
        /// </summary>
        public bool IsOutput { get; set; }
        /// <summary>
        /// Does the parameter allow nulls.
        /// </summary>
        public bool IsNullable { get; set; }
    }
}
