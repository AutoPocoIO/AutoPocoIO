using AutoPocoIO.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Dashboard.Extensions
{
    public static class ModelExtensions
    {
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

        private static object FindValue(Type type, string value)
        {
            object property = type.IsValueType ? Activator.CreateInstance(type) : null;

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (type == typeof(string))
                property = value;
            else if (type == typeof(bool) && value.Equals("on", System.StringComparison.OrdinalIgnoreCase))
                property = true;
            else
                property = converter.ConvertFromString(value);

            return property;
        }

        public static int ToInt(this Match match, string key)
        {
            Check.NotNull(match, nameof(match));
            return int.Parse(match.Groups[key].Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
        }

        public static string GetString(this Match match, string key)
        {
            Check.NotNull(match, nameof(match));
            return match.Groups[key].Value;
        }
    }
}
