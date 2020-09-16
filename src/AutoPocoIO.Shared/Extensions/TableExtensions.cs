using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.Exceptions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;


namespace AutoPocoIO.Extensions
{
    internal static class TableExtensions
    {
        public static IList<PrimaryKeyInformation> GetTableKeys(this IList<PrimaryKeyInformation> PKs, object[] keys)
        {
            for (int i = 0; i < PKs.Count; i++)
            {
                var PK = PKs[i];
                var keyValue = keys[i];

                if (keyValue is string)
                {
                    var converter = TypeDescriptor.GetConverter(PK.Type);
                    if (converter.IsValid(keyValue))
                        PK.Value = converter.ConvertFrom(keyValue);
                    else
                        throw new PrimaryKeyTypeMismatchException(PK, keyValue.ToString());
                }
                else if (keyValue.GetType() == PK.Type)
                    PK.Value = keyValue;
                else
                    throw new PrimaryKeyTypeMismatchException(PK, keyValue.GetType());
            }

            return PKs;
        }

        public static IList<PrimaryKeyInformation> GetTableKeys<TDbSet, TModel>(this IList<PrimaryKeyInformation> PKs, TModel entity) where TDbSet : class
        {
            foreach (var PK in PKs)
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