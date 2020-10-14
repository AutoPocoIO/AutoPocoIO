using AutoPocoIO.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Dashboard.Extensions
{
    /// <summary>
    /// Dashboard model extensions
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Get value from form values
        /// </summary>
        /// <typeparam name="TProperty">Expected property type</typeparam>
        /// <param name="form">Form values</param>
        /// <param name="key">Form property name</param>
        /// <returns></returns>
        public static TProperty FindValue<TProperty>(this IDictionary<string, string[]> form, string key)
        {
            Check.NotNull(form, nameof(form));
            Check.NotNull(key, nameof(key));

            object property = default(TProperty);
            if (form.ContainsKey(key))
            {
                var type = typeof(TProperty);
                if (type == typeof(string))
                    property = form[key][0];
                else if (typeof(IEnumerable).IsAssignableFrom(typeof(TProperty)))
                {
                    var parameter = Expression.Parameter(typeof(int));
                    var newArray = Expression.NewArrayBounds(type.GetElementType(), parameter);
                    var createArrayFunc = Expression.Lambda<Func<int, IList>>(newArray, parameter).Compile();
                    IList values = createArrayFunc(form[key].Length);

                    //insert func
                    ParameterExpression arrayExpr = Expression.Parameter(typeof(IList), "array");
                    ParameterExpression indexExpression = Expression.Parameter(typeof(int), "index");
                    ParameterExpression valueExpr = Expression.Parameter(typeof(object), "value");

                    var convertExpression = Expression.Convert(valueExpr, type.GetElementType());
                    Expression arrayAccessExpr = Expression.Property(arrayExpr, "Item", indexExpression);

                    var assignFunc = Expression.Lambda<Action<IList, int, object>>(
                        Expression.Assign(arrayAccessExpr, convertExpression),
                        arrayExpr,
                        indexExpression,
                        valueExpr).Compile();

                    for (int i = 0; i < form[key].Length; i++)
                    {
                        assignFunc(values, i, FindValue(type.GetElementType(), form[key][i]));
                    }
                    property = (TProperty)values;
                }
                else
                {
                    property = FindValue(type, form[key][0]);
                }
            }

            return (TProperty)property;
        }


        /// <summary>
        /// Get string value from Regex match
        /// </summary>
        /// <param name="match">Regex match</param>
        /// <param name="key">Group to parse to string</param>
        /// <returns></returns>
        public static string GetString(this Match match, string key)
        {
            Check.NotNull(match, nameof(match));
            return match.Groups[key].Value;
        }

        /// <summary>
        /// Get Guid value from Regex match
        /// </summary>
        /// <param name="match">Regex match</param>
        /// <param name="key">Group to parse to Guid</param>
        /// <returns></returns>
        public static Guid ToGuid(this Match match, string key)
        {
            Check.NotNull(match, nameof(match));
            return Guid.Parse(match.Groups[key].Value);
        }

        private static object FindValue(Type type, string value)
        {
            object property;

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (type == typeof(string))
                property = value;
            else if (type == typeof(bool) && value.Equals("on", System.StringComparison.OrdinalIgnoreCase))
                property = true;
            else
                property = converter.ConvertFromString(value);

            return property;
        }


    }
}
