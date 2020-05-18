using AutoPocoIO.DynamicSchema.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.DynamicSchema.Runtime
{
    internal static class ReflectionExtensions
    {
        public static IEnumerable<string> UserJoinedColumnSelect(Table table, Type type, IDictionary<string, string> queryString, string prependProperties = "")
        {
            var columns = table.Columns.Select(c => c.ColumnName);
            //Exlude objects becuase that means dynamic linq couldn't find the type
            var properties = type.GetProperties().Where(c => c.PropertyType != typeof(object));

            foreach (var property in properties)
            {
                if (columns.Any(c => c == property.Name))
                    yield return prependProperties + property.Name;
                else if (queryString.ContainsKey("$expand") && queryString["$expand"].Contains(property.Name))
                    yield return prependProperties + property.Name;
                else
                    yield return "Int32?(null) as " + property.Name;
            }
        }
    }
}