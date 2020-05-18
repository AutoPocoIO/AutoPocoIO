using AutoPocoIO.CustomAttributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.DynamicSchema.Services.CrossDb
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class SqlServerModelValidator : Microsoft.EntityFrameworkCore.Internal.SqlServerModelValidator
    {
        public SqlServerModelValidator(ModelValidatorDependencies dependencies, RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        protected override void ValidateSharedTableCompatibility(IReadOnlyList<IEntityType> mappedTypes, string tableName)
        {
            //check if same db
            var ofTable = mappedTypes.Select(c => new { Relational = c.Relational(), c.ClrType })
                                    .Where(c => ((string.IsNullOrEmpty(c.Relational.Schema) ? "" : c.Relational.Schema + ".") + c.Relational.TableName) == tableName);

            var dbCount = ofTable.Select(c => c.ClrType.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(DatabaseNameAttribute))
                                                                        ?.ConstructorArguments[0].Value.ToString())
                                     .Distinct();

            if (ofTable.Count() != dbCount.Count())
                base.ValidateSharedTableCompatibility(mappedTypes, tableName);
        }

        protected override void ValidateSharedColumnsCompatibility(IReadOnlyList<IEntityType> mappedTypes, string tableName)
        {
            //check if same db
            var ofTable = mappedTypes.Select(c => new { Relational = c.Relational(), c.ClrType })
                                    .Where(c => ((string.IsNullOrEmpty(c.Relational.Schema) ? "" : c.Relational.Schema + ".") + c.Relational.TableName) == tableName);

            var dbCount = ofTable.Select(c => c.ClrType.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(DatabaseNameAttribute))
                                                                        ?.ConstructorArguments[0].Value.ToString())
                                     .Distinct();

            if (ofTable.Count() != dbCount.Count())
                base.ValidateSharedColumnsCompatibility(mappedTypes, tableName);
        }
    }
}
