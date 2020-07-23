using AutoPocoIO.CustomAttributes;
using AutoPocoIO.DynamicSchema.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.Exceptions
{
    /// <summary>
    ///  Exception that sets status to 500 (Sever Error)
    /// </summary>
    public class PrimaryKeyNotFoundException : BaseCaughtException
    {
        private readonly IList<PrimaryKeyInformation> _keys;
        private readonly string _entityName;
        /// <summary>
        ///  Initialize exception with entity name
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="expectedKeys">Entity key information</param>
        public PrimaryKeyNotFoundException(string entityName, IList<PrimaryKeyInformation> expectedKeys) : base()
        {
            _keys = expectedKeys;
            _entityName = entityName;
        }
        /// <inheritdoc/>
        public override string Message => $"One or more of following properies were not found in the model for '{_entityName}'. Keys: {FormatKeys()}.";
        /// <summary>
        /// Status message is <c>EntityKeyNotFound</c>
        /// </summary>
        public override string HttpErrorMessage => "EntityKeyNotFound";

        private string FormatKeys()
        {
            return string.Join(", ", _keys.Select(c => c.Name));
        }

        public static string FormatEntityName(Type entityType)
        {
            return $"{GetAttribute<DatabaseNameAttribute>(entityType).DatabaseName}.{GetAttribute<TableAttribute>(entityType).Schema}.{GetAttribute<TableAttribute>(entityType).Name}";
        }

        private static T GetAttribute<T>(Type entityType) where T: Attribute
        {
           return entityType.GetCustomAttribute<T>();
        }
    }
}
