using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AutoPocoIO.Dashboard.Extensions
{
    internal static class ModelExtensions
    {
        public static TProperty FindValue<TProperty>(this IDictionary<string, string[]> form, string key)
        {
            object property = default(TProperty);
            if (form.ContainsKey(key))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(TProperty));
                if (typeof(TProperty) == typeof(string))
                    property = form[key][0];
                else if (typeof(TProperty) == typeof(bool) && form[key][0].Equals("on", System.StringComparison.OrdinalIgnoreCase))
                    property = true;
                else
                    property = (TProperty)converter.ConvertFromString(form[key][0]);

            }

            return (TProperty)property;
        }

        public static int ToInt(this Match match, string key)
        {
            return int.Parse(match.Groups[key].Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
        }
    }
}
