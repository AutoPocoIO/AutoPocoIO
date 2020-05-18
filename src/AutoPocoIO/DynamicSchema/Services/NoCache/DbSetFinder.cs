using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Services.NoCache
{
    [ExcludeFromCodeCoverage]
    internal class DbSetFinder : Microsoft.EntityFrameworkCore.Internal.DbSetFinder
    {
        public override IReadOnlyList<DbSetProperty> FindSets(DbContext context)
        {
            return FindSets(context.GetType());
        }

        private static DbSetProperty[] FindSets(Type contextType)
        {
            var factory = new ClrPropertySetterFactory();

            return contextType.GetRuntimeProperties()
                .Where(
                    p => !IsStatic(p)
                         && !p.GetIndexParameters().Any()
                         && p.DeclaringType != typeof(DbContext)
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && (p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                             || p.PropertyType.GetGenericTypeDefinition() == typeof(DbQuery<>)))
                .OrderBy(p => p.Name)
                .Select(
                    p => new DbSetProperty(
                        p.Name,
                        p.PropertyType.GetTypeInfo().GenericTypeArguments.Single(),
                        p.SetMethod == null ? null : factory.Create(p),
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbQuery<>)))
                .ToArray();
        }

        public static bool IsStatic(PropertyInfo property)
            => (property.GetMethod ?? property.SetMethod).IsStatic;

    }
}