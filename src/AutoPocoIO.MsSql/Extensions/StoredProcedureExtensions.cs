using System.Collections.Generic;
using System;

namespace AutoPocoIO.Extensions
{
    public static class StoredProcedureExtensions
    {
        /// <summary>
        /// Maps a result set to a view model.
        /// </summary>
        /// <typeparam name="TViewModel">View Model type</typeparam>
        /// <param name="outputParameters">Output from the stored procedure execution.</param>
        /// <param name="resultSetIndex">Zero based index of output set.  Appendeds index to "ResultSet" to create the parameter name. The first set can be zero or null.</param>
        /// <exception cref="ArgumentException">Thrown when the parameter is not found or not a result set.</exception>
        /// <returns></returns>
        public static IEnumerable<TViewModel> ProjectMsSqlResultSet<TViewModel>(this IDictionary<string, object> outputParameters, int? resultSetIndex = null)
        {
            string parameterName = "ResultSet" + resultSetIndex;
            return outputParameters.ProjectResultSet<TViewModel>(parameterName);
        }
    }
}
