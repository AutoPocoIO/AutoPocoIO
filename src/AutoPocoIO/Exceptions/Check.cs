using Microsoft.EntityFrameworkCore.Internal;
using System;

namespace AutoPocoIO.Exceptions
{
    public static class Check
    {
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", MessageId = "value")]
        //[ContractAnnotation("value:null => halt")]
        public static void NotNull<T>(T value, string parameterName)
        {
            // Contract.Requires<ArgumentNullException>(value != null, parameterName);
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }

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