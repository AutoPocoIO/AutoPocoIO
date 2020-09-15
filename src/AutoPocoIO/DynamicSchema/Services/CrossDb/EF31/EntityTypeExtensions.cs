using AutoPocoIO.CustomAttributes;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Extensions
{
    internal static class EntityTypeExtensions
    {
        public static string GetDatabase(this IEntityType entityType)
        {
            return entityType.GetType().CustomAttributes
                                          .FirstOrDefault(c => c.AttributeType == typeof(DatabaseNameAttribute))
                                          ?.ConstructorArguments[0]
                                          .Value
                                          .ToString();
        }

        public static Type MakeNullable(this Type type, bool nullable = true)
          => type.IsNullableType() == nullable
              ? type
              : nullable
                  ? typeof(Nullable<>).MakeGenericType(type)
                  : type.UnwrapNullableType();

        public static Type UnwrapNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public static bool IsNullableValueType(this Type type)
           => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static bool IsNullableType(this Type type)
          => !type.IsValueType || type.IsNullableValueType();

        public static bool IsStatic(this PropertyInfo property)
           => (property.GetMethod ?? property.SetMethod).IsStatic;
    }
}
