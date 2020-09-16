using System;

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// Used by EF visit table to query across same server databases
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseNameAttribute : Attribute
    {
        private readonly string _databaseName;
        /// <summary>
        /// Initializes a new instance of the AutoPoco.CustomAttributes.DatabaseNameAttribute class
        /// </summary>
        /// <param name="databaseName">The name of the Database</param>
        public DatabaseNameAttribute(string databaseName)
        {
            _databaseName = databaseName;
        }

        /// <summary>
        /// The name of the Database
        /// </summary>
        public string DatabaseName => _databaseName;
    }
}
