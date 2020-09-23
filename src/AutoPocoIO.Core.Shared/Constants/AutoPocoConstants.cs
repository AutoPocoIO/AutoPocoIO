namespace AutoPocoIO
{
    /// <summary>
    /// Application constants
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
    public static class AutoPocoConstants
    {
        /// <summary>
        /// Connector name defaults for migrations.
        /// </summary>
        public static class DefaultConnectors
        {
            /// <summary>
            /// Application database name.
            /// </summary>
            public const string AppDB = "appDb";
            /// <summary>
            /// Log database connector name.
            /// </summary>
            public const string Logging = "logDb";
        }
        /// <summary>
        /// Table name defaults for migrations.
        /// </summary>
        public static class DefaultTables
        {
            /// <summary>
            /// Connector table name
            /// </summary>
            public const string Connectors = "Connector";
            /// <summary>
            /// Request table name
            /// </summary>
            public const string RequestLogs = "Request";
            /// <summary>
            /// Response log table name.
            /// </summary>
            public const string ResponseLogs = "Response";
            /// <summary>
            /// User join table name.
            /// </summary>
            public const string UserJoins = "UserJoin";
        }
    }
}
