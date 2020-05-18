using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace AutoPocoIO.DynamicSchema.Services.NoCache
{
    [ExcludeFromCodeCoverage]
    internal class SqlServerTypeMappingSource : Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerTypeMappingSource
    {
        public SqlServerTypeMappingSource(TypeMappingSourceDependencies dependencies, RelationalTypeMappingSourceDependencies relationalDependencies) : base(dependencies, relationalDependencies)
        {
        }

        protected override RelationalTypeMapping FindMappingWithConversion(
           in RelationalTypeMappingInfo mappingInfo,
            IReadOnlyList<IProperty> principals)
        {
            Type providerClrType = null;
            ValueConverter customConverter = null;
            if (principals != null)
            {
                for (var i = 0; i < principals.Count; i++)
                {
                    var principal = principals[i];
                    if (providerClrType == null)
                    {
                        var providerType = principal.GetProviderClrType();
                        if (providerType != null)
                        {
                            providerClrType = providerType.UnwrapNullableType();
                        }
                    }

                    if (customConverter == null)
                    {
                        var converter = principal.GetValueConverter();
                        if (converter != null)
                        {
                            customConverter = converter;
                        }
                    }
                }
            }

            var resolvedMapping = NoCacheResolveTypeMapping(mappingInfo, providerClrType, customConverter);

            ValidateMapping(resolvedMapping, principals?[0]);

            return resolvedMapping;
        }

        private RelationalTypeMapping NoCacheResolveTypeMapping(RelationalTypeMappingInfo info, Type providerType, ValueConverter converter)
        {
            var mapping = providerType == null
                          || providerType == info.ClrType
                ? FindMapping(info)
                : null;

            if (mapping == null)
            {
                var sourceType = info.ClrType;

                if (sourceType != null)
                {
                    foreach (var converterInfo in Dependencies
                        .ValueConverterSelector
                        .Select(sourceType, providerType))
                    {
                        var mappingInfoUsed = info.WithConverter(converterInfo);
                        mapping = FindMapping(mappingInfoUsed);

                        if (mapping == null
                            && providerType != null)
                        {
                            foreach (var secondConverterInfo in Dependencies
                                .ValueConverterSelector
                                .Select(providerType))
                            {
                                mapping = FindMapping(mappingInfoUsed.WithConverter(secondConverterInfo));

                                if (mapping != null)
                                {
                                    mapping = (RelationalTypeMapping)mapping.Clone(secondConverterInfo.Create());
                                    break;
                                }
                            }
                        }

                        if (mapping != null)
                        {
                            mapping = (RelationalTypeMapping)mapping.Clone(converterInfo.Create());
                            break;
                        }
                    }
                }
            }

            if (mapping != null
                && converter != null)
            {
                mapping = (RelationalTypeMapping)mapping.Clone(converter);
            }

            return mapping;
        }



    }

    [ExcludeFromCodeCoverage]
    internal static class TypeMappingExcentions
    {
        public static Type UnwrapNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
    }


}
