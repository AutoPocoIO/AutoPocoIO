using Microsoft.EntityFrameworkCore.Internal;
using System;

namespace AutoPocoIO.Exceptions
{
    /// <summary>
    /// Check if values are null/empty
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Throw an excpetion if value is not null.
        /// </summary>
        /// <typeparam name="T">Type of parameter</typeparam>
        /// <param name="value">Parameter to check</param>
        /// <param name="parameterName">Parameter name</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull<T>(T value, string parameterName)
        {
            // Contract.Requires<ArgumentNullException>(value != null, parameterName);
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Throw an exception if value is null or empty.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="parameterName">Method parameter name.</param>
        /// <returns></returns>
        public static string NotEmpty(string value, string parameterName)
        {
            Exception e = null;
            if (value is null)
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Trim().Length == 0)
            {
                e = new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }

            if (e != null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }
    }
}