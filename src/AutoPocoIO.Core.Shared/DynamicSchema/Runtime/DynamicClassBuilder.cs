using AutoPocoIO.CustomAttributes;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;

namespace AutoPocoIO.DynamicSchema.Runtime
{
    internal class DynamicClassBuilder
    {
        private readonly IDbSchema _schema;
        private readonly Dictionary<string, Type> _types;
        private readonly Dictionary<string, TypeBuilder> _typeBuilders;

        private const string ObjectPostfixName = "Object";
        private const string CollectionPostfixName = "List";

        public virtual Dictionary<string, Type> ExistingAssemblies { get; private set; }

        public DynamicClassBuilder(IDbSchema schema)
        {
            Check.NotNull(schema, nameof(schema));

            _schema = schema;
            _types = new Dictionary<string, Type>();
            _typeBuilders = new Dictionary<string, TypeBuilder>(StringComparer.OrdinalIgnoreCase);


            ExistingAssemblies = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        }

        public virtual void CreateModelTypes(string tableName)
        {
            _types.Clear();
            _typeBuilders.Clear();
            ExistingAssemblies.Clear();

            int requestHash = _schema.GetHashCode();

            //Verify if all Types exists
            bool allTypesExist = true;
            PopulateExistingAssemblies(_schema.Tables, tableName, requestHash, ref allTypesExist);
            PopulateExistingAssemblies(_schema.Views, tableName, requestHash, ref allTypesExist);

            if (!allTypesExist)
            {
                ExistingAssemblies.Clear();

                //Create Normal Poco Type to be used as a reference
                CreateTypes(_schema.Tables, tableName, requestHash);
                CreateTypes(_schema.Views, tableName, requestHash);


                //Navigation properties
                CreateNavigationProperties(_schema.Tables);

                //Creates DbSet Propeties for the Context
                CreateDBSetProperty(_schema.Tables, tableName, requestHash);
                CreateDBSetProperty(_schema.Views, tableName, requestHash);
            }
        }

        private void PopulateExistingAssemblies<T>(List<T> tables, string tableName, int requestHash, ref bool allTypesExist) where T : Table
        {
            if (allTypesExist)
            {
                foreach (Table table in tables)
                {
                    var assemblyName = Utils.AssemblyName(table, tableName, requestHash);
                    try
                    {
                        //Check if assembly finished creation, concurrency when multi requests run async it tries to create the assembly multiple time due it not existing.
                        var foundType = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase) && x.IsDynamic).FindLoadedAssembly(true);

                        if (foundType != null)
                        {
                            //add created type to list for dbcontext
                            ExistingAssemblies.Add(assemblyName, foundType);
                        }
                        else
                        {
                            //create a duplicate assembly so thread doesnt fail if main assembly is still being created.
                            allTypesExist = false;
                            return;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        //if lookup fails, that means another thread has created the assembly but hasn't finished creating the type. Spawn new type.
                        allTypesExist = false;
                        return;
                    }

                }
            }
        }

        private void CreateTypes<T>(List<T> tables, string tableName, int requestHash) where T : Table
        {
            foreach (Table table in tables)
            {
                var assemblyName = Utils.AssemblyName(table, tableName, requestHash);
                _typeBuilders.Add(table.VariableName, CreatePocoTypeBuilder(table, assemblyName));
            }
        }

        private void CreateNavigationProperties<T>(List<T> tables) where T : Table
        {
            foreach (Table table in tables)
            {
                TypeBuilder FKTypeBuilder;
                TypeBuilder CollectionTypeBuilder;
                TypeBuilder builder;
                PropertyBuilder pb;
                CustomAttributeBuilder attribute;
                foreach (Column column in table.Columns)
                {

                    if (_typeBuilders.TryGetValue(column.ReferencedVariableName, out TypeBuilder tbReferencedVariableName)
                        && _typeBuilders.TryGetValue(table.VariableName, out TypeBuilder tbVariableName))
                    {

                        if (column.IsFK)
                        {
                            builder = tbVariableName;
                            //Createing FK Object
                            FKTypeBuilder = tbReferencedVariableName;


                            pb = DynamicTypeBuilder.CreateVirtualProperty(builder, DependentToPrimaryObjectName(column, table), FKTypeBuilder);

                            //DisplayName Attribute
                            ConstructorInfo DisplayNameAttributeBuilder = typeof(DisplayNameAttribute).GetConstructor(new Type[] { typeof(string) });
                            attribute = new CustomAttributeBuilder(DisplayNameAttributeBuilder, new object[] { Utils.GetFancyLabel(DependentToPrimaryObjectName(column, table)) });
                            pb.SetCustomAttribute(attribute);

                            //Browsable Attribute
                            ConstructorInfo BrowsableAttributeBuilder = typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(bool) });
                            attribute = new CustomAttributeBuilder(BrowsableAttributeBuilder, new object[] { true });
                            pb.SetCustomAttribute(attribute);

                            //DataMember Attribute
                            ConstructorInfo DataMemberAttributeBuilder = typeof(DataMemberAttribute).GetConstructor(Array.Empty<Type>());
                            attribute = new CustomAttributeBuilder(DataMemberAttributeBuilder, Array.Empty<object>());
                            pb.SetCustomAttribute(attribute);

                            //ReferencedDbObject Attribute
                            ConstructorInfo ReferencedDbObjectAttributeBuilder = typeof(ReferencedDbObjectAttribute).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string) });
                            attribute = new CustomAttributeBuilder(ReferencedDbObjectAttributeBuilder, new object[] { column.ReferencedDatabase, column.ReferencedSchema, column.ReferencedTable });
                            pb.SetCustomAttribute(attribute);

                            //Reference table object
                            builder = tbReferencedVariableName;
                            CollectionTypeBuilder = tbVariableName;
                            if (column.IsPK && table.Columns.Where(c => c.IsPK).Select(c => c.ReferencedTable).Distinct().Count() == 1)
                            {
                                //Create a single object for 1:1 relationships on when all PKs point to the same object
                                pb = DynamicTypeBuilder.CreateVirtualProperty(builder, PrimaryToDepdenty1To1ObjectName(column), CollectionTypeBuilder.UnderlyingSystemType);

                                //DisplayName Attribute
                                ConstructorInfo DisplayNameAttributeBuilder2 = typeof(DisplayNameAttribute).GetConstructor(new Type[] { typeof(string) });
                                attribute = new CustomAttributeBuilder(DisplayNameAttributeBuilder2, new object[] { Utils.GetFancyLabel(PrimaryToDepdenty1To1ObjectName(column)) });
                                pb.SetCustomAttribute(attribute);
                            }
                            else
                            {
                                //Creating Collection Object for the referenced table
                                pb = DynamicTypeBuilder.CreateVirtualProperty(builder, PrimaryToDependenty1ToManyListName(column, table), typeof(CustomList<>).MakeGenericType(new Type[] { CollectionTypeBuilder.UnderlyingSystemType }));


                                //InverseProperty Attribute
                                ConstructorInfo InversePropertyAttributeBuilder = typeof(InversePropertyAttribute).GetConstructor(new Type[] { typeof(string) });
                                attribute = new CustomAttributeBuilder(InversePropertyAttributeBuilder, new object[] { DependentToPrimaryObjectName(column, table) });
                                pb.SetCustomAttribute(attribute);

                                //DisplayName Attribute
                                ConstructorInfo DisplayNameAttributeBuilder2 = typeof(DisplayNameAttribute).GetConstructor(new Type[] { typeof(string) });
                                attribute = new CustomAttributeBuilder(DisplayNameAttributeBuilder2, new object[] { Utils.GetFancyLabel(PrimaryToDependenty1ToManyListName(column, table)) });
                                pb.SetCustomAttribute(attribute);
                            }

                            //Browsable Attribute
                            ConstructorInfo BrowsableAttributeBuilder2 = typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(bool) });
                            attribute = new CustomAttributeBuilder(BrowsableAttributeBuilder2, new object[] { true });
                            pb.SetCustomAttribute(attribute);

                            //DataMember Attribute
                            ConstructorInfo DataMemberAttributeBuilder2 = typeof(System.Runtime.Serialization.DataMemberAttribute).GetConstructor(Array.Empty<Type>());
                            attribute = new CustomAttributeBuilder(DataMemberAttributeBuilder2, Array.Empty<object>());
                            pb.SetCustomAttribute(attribute);

                            //ReferencedDbObject Attribute
                            ConstructorInfo ReferencedDbObjectAttributeBuilder2 = typeof(ReferencedDbObjectAttribute).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string) });
                            attribute = new CustomAttributeBuilder(ReferencedDbObjectAttributeBuilder2, new object[] { table.Database, table.Schema, table.Name });
                            pb.SetCustomAttribute(attribute);
                        }
                    }


                }
            }
        }

        private static TypeBuilder CreatePocoTypeBuilder(Table table, string assemblyName)
        {
            Type PropertyType;
            PropertyBuilder propertyBuilder;
            PropertyInfo pi;

            TypeBuilder builder = DynamicTypeBuilder.GetTypeBuilder(table, typeof(PocoBase), assemblyName);

            ConstructorBuilder constructor = builder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            //DataContract Attribute
            ConstructorInfo DataContractAttributeBuilder = typeof(DataContractAttribute).GetConstructor(Array.Empty<Type>());
            pi = typeof(DataContractAttribute).GetProperties().FirstOrDefault(o => o.Name == nameof(DataContractAttribute.IsReference));
            var attribute = new CustomAttributeBuilder(DataContractAttributeBuilder, Array.Empty<object>(), new PropertyInfo[] { pi }, new object[] { false });
            builder.SetCustomAttribute(attribute);

            //Table Schema Attribute
            ConstructorInfo TableAttributeBuilder = typeof(TableAttribute).GetConstructor(new Type[] { typeof(string) });
            pi = typeof(TableAttribute).GetProperties().FirstOrDefault(o => o.Name == nameof(TableAttribute.Schema));
            attribute = new CustomAttributeBuilder(TableAttributeBuilder, new object[] { table.TableAttributeName }, new PropertyInfo[] { pi }, new object[] { table.Schema });
            builder.SetCustomAttribute(attribute);

            //Db attribute
            ConstructorInfo DatabaseNameAttributeBuilder = typeof(DatabaseNameAttribute).GetConstructor(new Type[] { typeof(string) });
            attribute = new CustomAttributeBuilder(DatabaseNameAttributeBuilder, new object[] { table.Database });
            builder.SetCustomAttribute(attribute);

            //Creating normal properties for each poco class
            bool hasPKColumn = false;
            foreach (Column column in table.Columns)
            {
                PropertyType = column.DataType.SystemType;
                string propertyName = column.ColumnToPropertyName();

                propertyBuilder = DynamicTypeBuilder.CreateProperty(builder, propertyName, PropertyType, true);


                //DisplayName Attribute
                ConstructorInfo DisplayNameAttributeBuilder = typeof(DisplayNameAttribute).GetConstructor(new Type[] { typeof(string) });
                attribute = new CustomAttributeBuilder(DisplayNameAttributeBuilder, new object[] { Utils.GetFancyLabel(column.ColumnName) });
                propertyBuilder.SetCustomAttribute(attribute);

                //Column Attribute
                ConstructorInfo ColumnAttributeBuilder = typeof(ColumnAttribute).GetConstructor(new Type[] { typeof(string) });

                //set column datatype to varchar to prevent nvarchar casting
                if (column.ColumnType.Equals("varchar", StringComparison.OrdinalIgnoreCase))
                {
                    pi = typeof(ColumnAttribute).GetProperties().FirstOrDefault(o => o.Name == nameof(ColumnAttribute.TypeName));
                    attribute = new CustomAttributeBuilder(ColumnAttributeBuilder, new object[] { column.ColumnName }, new PropertyInfo[] { pi }, new object[] { $"varchar({column.ColumnLength})" });
                }
                else
                    attribute = new CustomAttributeBuilder(ColumnAttributeBuilder, new object[] { column.ColumnName });

                propertyBuilder.SetCustomAttribute(attribute);


                if (column.IsPK)
                {
                    //Key Attribute (using AutoPocoOdata custom attribute)
                    //EF Core does not allow compound [Key] attributes in attributes
                    ConstructorInfo KeyAttributeBuilder = typeof(CompoundPrimaryKeyAttribute).GetConstructor(new Type[] { typeof(int) });
                    attribute = new CustomAttributeBuilder(KeyAttributeBuilder, new object[] { column.PKPosition });
                    propertyBuilder.SetCustomAttribute(attribute);

                    //Set to prevent a fake pk from being added (views when PK is not selected)
                    hasPKColumn = true;

                    ConstructorInfo IdentityAttributeBuilder = typeof(DatabaseGeneratedAttribute).GetConstructor(new Type[] { typeof(DatabaseGeneratedOption) });
                    if (!column.PKIsIdentity)
                        attribute = new CustomAttributeBuilder(IdentityAttributeBuilder, new object[] { DatabaseGeneratedOption.None });
                    else
                        attribute = new CustomAttributeBuilder(IdentityAttributeBuilder, new object[] { DatabaseGeneratedOption.Identity });

                    propertyBuilder.SetCustomAttribute(attribute);
                }

                if (column.IsFK)
                {
                    //foreignKey Attribute
                    ConstructorInfo foreignKeyAttributeBuilder = typeof(ForeignKeyAttribute).GetConstructor(new Type[] { typeof(string) });
                    attribute = new CustomAttributeBuilder(foreignKeyAttributeBuilder, new object[] { DependentToPrimaryObjectName(column, table) });
                    propertyBuilder.SetCustomAttribute(attribute);

                    //ReferencedDbObject attribute
                    ConstructorInfo referencedDbObjectAttributeBuilder = typeof(ReferencedDbObjectAttribute).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
                    attribute = new CustomAttributeBuilder(referencedDbObjectAttributeBuilder, new object[] { column.ReferencedDatabase, column.ReferencedSchema, column.ReferencedTable, column.ReferencedColumn });
                    propertyBuilder.SetCustomAttribute(attribute);
                }

                //DataMember Attribute
                ConstructorInfo DataMemberAttributeBuilder = typeof(DataMemberAttribute).GetConstructor(Array.Empty<Type>());
                attribute = new CustomAttributeBuilder(DataMemberAttributeBuilder, Array.Empty<object>());
                propertyBuilder.SetCustomAttribute(attribute);

                //Computed Column Attribute
                if (column.IsComputed)
                {
                    ConstructorInfo ComputedAttributeBuilder = typeof(DatabaseGeneratedAttribute).GetConstructor(new Type[] { typeof(DatabaseGeneratedOption) });
                    attribute = new CustomAttributeBuilder(ComputedAttributeBuilder, new object[] { DatabaseGeneratedOption.Computed });
                    propertyBuilder.SetCustomAttribute(attribute);
                }

                bool Browsable = column.Browsable;
                Browsable = Browsable | (column.IsFK) | (column.IsPK);
                //Browsable Attribute
                ConstructorInfo BrowsableAttributeBuilder = typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(bool) });
                attribute = new CustomAttributeBuilder(BrowsableAttributeBuilder, new object[] { Browsable });
                propertyBuilder.SetCustomAttribute(attribute);

                //Required Attribute
                if (!column.ColumnIsNullable)
                {
                    ConstructorInfo RequiredeAttributeBuilder = typeof(RequiredAttribute).GetConstructor(Array.Empty<Type>());
                    attribute = new CustomAttributeBuilder(RequiredeAttributeBuilder, Array.Empty<object>());
                    propertyBuilder.SetCustomAttribute(attribute);
                }
            }

            if (!hasPKColumn && table is View)
            {
                //Create Fake pk
                //Key Attribute
                propertyBuilder = DynamicTypeBuilder.CreateProperty(builder, "virtualPK", typeof(int), false);

                ConstructorInfo KeyAttributeBuilder = typeof(KeyAttribute).GetConstructor(Array.Empty<Type>());
                attribute = new CustomAttributeBuilder(KeyAttributeBuilder, Array.Empty<object>());
                propertyBuilder.SetCustomAttribute(attribute);
            }

            return builder;
        }

        private void CreateDBSetProperty<T>(List<T> tables, string tableName, int requestHash) where T : Table
        {
            Type PocoType;
            TypeBuilder PocoTypeBuilder;

            foreach (Table table in tables)
            {

                if (_typeBuilders.TryGetValue(table.VariableName, out TypeBuilder tbVariableName))
                {
                    PocoTypeBuilder = tbVariableName;
                    PocoType = PocoTypeBuilder.CreateTypeInfo().AsType();
                    _types.Add(table.VariableName, PocoType);

                    //add created types to the list to pass to the dbcontext
                    var assemblyName = Utils.AssemblyName(table, tableName, requestHash);
                    ExistingAssemblies.Add(assemblyName, PocoType);
                }
            }
        }

        internal static string DependentToPrimaryObjectName(Column column, Table table)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendUppercaseFirst(column.UserDefinedFKAlias ?? column.ReferencedTable);
            builder.Append(ObjectPostfixName);
            builder.Append("From");
            builder.AppendUppercaseFirst(table.GenerateFKName(column));
            return builder.ToString();
        }

        internal static string PrimaryToDepdenty1To1ObjectName(Column column)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendUppercaseFirst(column.UserDefinedFKAlias ?? column.TableName);
            builder.Append(ObjectPostfixName);
            builder.Append("From");
            builder.AppendUppercaseFirst(column.ReferencedColumn);
            return builder.ToString();
        }

        internal static string PrimaryToDependenty1ToManyListName(Column column, Table table)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendUppercaseFirst(column.UserDefinedFKAlias ?? column.TableName);
            builder.Append(CollectionPostfixName);
            builder.Append("From");
            builder.AppendUppercaseFirst(table.GenerateFKName(column));
            return builder.ToString();
        }
    }
}