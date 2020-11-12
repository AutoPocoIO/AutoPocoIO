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

        public override IReadOnlyList<DbSetProperty> FindSets(Type contextType)
        {
            return FindSetsNonCached(contextType);
        }

        private static DbSetProperty[] FindSetsNonCached(Type contextType)
        {
            var factory = new ClrPropertySetterFactory();

            return contextType.GetRuntimeProperties()
                .Where(
                    p => !IsStatic(p)
                         && !p.GetIndexParameters().Any()
                         && p.DeclaringType != typeof(DbContext)
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && (p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
#pragma warning disable CS0618 // Type or member is obsolete
                             || p.PropertyType.GetGenericTypeDefinition() == typeof(DbQuery<>)))
#pragma warning restore CS0618 // Type or member is obsolete
                .OrderBy(p => p.Name)
                .Select(
                    p => new DbSetProperty(
                        p.Name,
                        p.PropertyType.GetTypeInfo().GenericTypeArguments.Single(),
                        p.SetMethod == null ? null : factory.Create(p),
#pragma warning disable CS0618 // Type or member is obsolete
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbQuery<>)))
#pragma warning restore CS0618 // Type or member is obsolete
                .ToArray();
        }

        public static bool IsStatic(PropertyInfo property)
            => (property.GetMethod ?? property.SetMethod).IsStatic;

    }
}