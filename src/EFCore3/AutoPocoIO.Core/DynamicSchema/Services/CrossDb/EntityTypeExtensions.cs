using AutoPocoIO.CustomAttributes;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Extensions
{
    internal static class EntityTypeExtensions
    {
        public static string GetDatabase(this IEntityType entityType)
        {
            return entityType.ClrType.CustomAttributes
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

        public static Type TryGetSequenceType(this Type type)
       => type.TryGetElementType(typeof(IEnumerable<>))
           ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));


        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (type.GetTypeInfo().IsGenericTypeDefinition)
            {
                return null;
            }

            var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

            Type singleImplementation = null;
            foreach (var implementation in types)
            {
                if (singleImplementation == null)
                {
                    singleImplementation = implementation;
                }
                else
                {
                    singleImplementation = null;
                    break;
                }
            }

            return singleImplementation?.GetTypeInfo().GenericTypeArguments.FirstOrDefault();
        }

        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : type.GetBaseTypes();
                foreach (var baseType in baseTypes)
                {
                    if (baseType.GetTypeInfo().IsGenericType
                        && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return baseType;
                    }
                }

                if (type.GetTypeInfo().IsGenericType
                    && type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            type = type.GetTypeInfo().BaseType;

            while (type != null)
            {
                yield return type;

                type = type.GetTypeInfo().BaseType;
            }
        }
    }
}
