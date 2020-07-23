using AutoPocoIO.DynamicSchema.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System;
using AutoPocoIO.Exceptions;

namespace AutoPocoIO.Extensions
{
    internal static class TableExtensions
    {
        public static IList<PrimaryKeyInformation> GetTableKeys(this IList<PrimaryKeyInformation> PKs, string keys)
        {
            var arrKeys = keys.Split(';');

            for (int i = 0; i < arrKeys.Length; i++)
            {
                var PK = PKs[i];
                var keyValue = arrKeys[i];

                var converter = TypeDescriptor.GetConverter(PK.Type);
                PK.Value = converter.ConvertFrom(keyValue);
            }

            return PKs;
        }

        public static IList<PrimaryKeyInformation> GetTableKeys<TDbSet, TModel>(this IList<PrimaryKeyInformation> PKs, TModel entity) where TDbSet : class
        {
            foreach(var PK in PKs)
            {
                var property = typeof(TModel).GetProperty(PK.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                {
                    string entityName = PrimaryKeyNotFoundException.FormatEntityName(typeof(TDbSet));
                    throw new PrimaryKeyNotFoundException(entityName, PKs);
                }
                    
                PK.Value = property.GetValue(entity);
            }

            return PKs;
        }
    }
}