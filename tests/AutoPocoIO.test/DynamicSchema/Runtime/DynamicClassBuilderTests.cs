using AutoPocoIO.CustomAttributes;
using AutoPocoIO.DynamicSchema.Db;
using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Xunit;
using DataType = AutoPocoIO.DynamicSchema.Models.DataType;

namespace AutoPocoIO.test.DynamicSchema.Runtime
{
    
    [Trait("Category", TestCategories.Unit)]
    public class DynamicClassBuilderTests
    {
        Guid guid1 = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();
        readonly Mock<DbSchema> schema;
        public DynamicClassBuilderTests()
        {
            var variableName = $"tbl_{guid1}";
            var table = new Mock<Table>();
            table.Setup(c => c.VariableName).Returns(variableName);
            table.Setup(c => c.TableAttributeName).Returns("tbl");
            table.Setup(c => c.GetHashCode()).Returns(123456);

            table.Setup(c => c.Columns).Returns(new List<Column>
            {
                new Column{ColumnName = "strCol", ColumnIsNullable = true, ColumnType="varchar", ColumnLength = 5, DataType = new DataType{SystemType = typeof(string) } },
                new Column{ColumnName = "intCol", ColumnIsNullable = true, ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "reqCol", ColumnIsNullable = false, ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "pkCol", PKName = "pk1", ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "pkColIdentity", PKName = "pk2", PKIsIdentity = true, ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "fkCol", FKName = "fk1", ReferencedTable="tbl123", ReferencedSchema="sch2", ReferencedDatabase="db3",  ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "fkColAlias", FKName = "fk2",  UserDefinedFKAlias = "alias1",  ReferencedTable="tbl123", ReferencedSchema="sch2", ReferencedDatabase="db3", ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "fkCol31", FKName = "fk3", ReferencedTable="tbl12",   ReferencedSchema="sch2", ReferencedDatabase="db3", ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "fkCol32", FKName = "fk3", ReferencedTable="tbl12",  ReferencedSchema="sch2", ReferencedDatabase="db3", ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
                new Column{ColumnName = "compCol", IsComputed = true, ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
            });

            table.Object.Database = "db1";
            table.Object.Schema = "sch1";


            schema = new Mock<DbSchema>();
            schema.Setup(c => c.Tables).Returns(new List<Table> { table.Object });
            schema.Setup(c => c.Views).Returns(new List<View>());
            schema.Setup(c => c.GetHashCode()).Returns(987654);
        }

        [FactWithName]
        public void AllTypesFound()
        {
            //Create so already found
            string asmName = $"dynamicassembly.tbl_{guid1}123456.reqTbl987654";
            string typename = asmName.Replace("dynamicassembly", "DynamicType");
            CreateAsm(asmName, $"tbl_{guid1}");

            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            Assert.Single(classBuilder.ExistingAssemblies);
            Assert.Equal($"tbl_{guid1}", classBuilder.ExistingAssemblies[asmName.ToUpperInvariant()].Name);
            Assert.Single(AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.ToUpperInvariant() == asmName.ToUpperInvariant()));
        }

        [FactWithName]
        public void RebuildAllTypesIfInTheMiddleOfCreatingType()
        {
            //Create so already found
            string asmName = $"dynamicassembly.tbl_{guid1}123456.reqTbl987654";
            string typename = asmName.Replace("dynamicassembly", "DynamicType");

            CreateAsm(asmName, $"tbl_{guid1}", createType: false);

            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            Assert.Single(classBuilder.ExistingAssemblies);
            Assert.Equal($"TBL_{guid1.ToString().ToUpper()}", classBuilder.ExistingAssemblies[asmName.ToUpperInvariant()].Name);
        }

        [FactWithName]
        public void RebuildAllTypesIfOnlySomeTimesFound()
        {
            var variableName = $"tbl12_{guid1}";
            var table = new Mock<Table>() { CallBase = true };
            table.Setup(c => c.VariableName).Returns(variableName);
            table.Setup(c => c.GetHashCode()).Returns(1234566);

            var table2 = new Mock<Table>() { CallBase = true };
            table2.Setup(c => c.VariableName).Returns($"tbl2_{guid2}");
            table2.Setup(c => c.GetHashCode()).Returns(4561233);

            schema.Setup(c => c.Tables).Returns(new List<Table> { table.Object, table2.Object });

            //Create so already found
            string asmName = $"dynamicassembly.tbl12_{guid1}1234566.reqTbl987654";
            CreateAsm(asmName, $"tbl12_{guid1}");

            Assert.Single(AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.ToUpperInvariant() == asmName.ToUpperInvariant()));
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            Assert.Equal(2, classBuilder.ExistingAssemblies.Count());
            //all upper means generated
            Assert.Equal($"TBL12_{guid1.ToString().ToUpper()}", classBuilder.ExistingAssemblies[asmName.ToUpperInvariant()].Name);
            Assert.Equal(2, AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.ToUpperInvariant() == asmName.ToUpperInvariant()).Count());
        }

        [FactWithName]
        public void TableLevelProperties()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var type = classBuilder.ExistingAssemblies.First().Value;

            Assert.False(type.GetCustomAttribute<DataContractAttribute>().IsReference);
            Assert.Equal("tbl", type.GetCustomAttribute<TableAttribute>().Name);
            Assert.Equal("sch1", type.GetCustomAttribute<TableAttribute>().Schema);
            Assert.Equal("db1", type.GetCustomAttribute<DatabaseNameAttribute>().DatabaseName);
        }

        [FactWithName]
        public void StringColumnsProperties()
        {

            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("strCol");

            Assert.Equal("Str Col", property.GetCustomAttribute<DisplayNameAttribute>().DisplayName);
            Assert.Equal("strCol", property.GetCustomAttribute<ColumnAttribute>().Name);
            Assert.Equal("varchar(5)", property.GetCustomAttribute<ColumnAttribute>().TypeName);
             Assert.NotNull(property.GetCustomAttribute<DataMemberAttribute>());
             Assert.True(property.GetCustomAttribute<BrowsableAttribute>().Browsable);
        }

        [FactWithName]
        public void IntColumnsProperties()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("intCol");

            Assert.Equal("Int Col", property.GetCustomAttribute<DisplayNameAttribute>().DisplayName);
            Assert.Equal("intCol", property.GetCustomAttribute<ColumnAttribute>().Name);
            Assert.Null(property.GetCustomAttribute<ColumnAttribute>().TypeName);
             Assert.NotNull(property.GetCustomAttribute<DataMemberAttribute>());
             Assert.True(property.GetCustomAttribute<BrowsableAttribute>().Browsable);
        }

        [FactWithName]
        public void RequiredColumnAttrIfColumnIsNullableIsTrue()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("reqCol");

             Assert.NotNull(property.GetCustomAttribute<RequiredAttribute>());
        }

        [FactWithName]
        public void PkColumnWithoutIdentity()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("pkCol");

             Assert.NotNull(property.GetCustomAttribute<CompoundPrimaryKeyAttribute>());
            Assert.Equal(DatabaseGeneratedOption.None, property.GetCustomAttribute<DatabaseGeneratedAttribute>().DatabaseGeneratedOption);
        }

        [FactWithName]
        public void PkColumnWithIdentity()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("pkColIdentity");

             Assert.NotNull(property.GetCustomAttribute<CompoundPrimaryKeyAttribute>());
            Assert.Equal(DatabaseGeneratedOption.Identity, property.GetCustomAttribute<DatabaseGeneratedAttribute>().DatabaseGeneratedOption);
        }

        [FactWithName]
        public void FkColumnWithoutAlias()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("fkCol");

            Assert.Equal("tbl123fkColObject", property.GetCustomAttribute<ForeignKeyAttribute>().Name);
            Assert.Equal("db3", property.GetCustomAttribute<ReferencedDbObjectAttribute>().DbName);
            Assert.Equal("sch2", property.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName);
            Assert.Equal("tbl123", property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName);
        }

        [FactWithName]
        public void FkColumnWithAlias()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("fkColAlias");

            Assert.Equal("alias1fkColAliasObject", property.GetCustomAttribute<ForeignKeyAttribute>().Name);
            Assert.Equal("db3", property.GetCustomAttribute<ReferencedDbObjectAttribute>().DbName);
            Assert.Equal("sch2", property.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName);
            Assert.Equal("tbl123", property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName);
        }


        [FactWithName]
        public void FkColumnCompoundKey()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("fkCol31");

            Assert.Equal("tbl12fkCol31AndfkCol32Object", property.GetCustomAttribute<ForeignKeyAttribute>().Name);
            Assert.Equal("db3", property.GetCustomAttribute<ReferencedDbObjectAttribute>().DbName);
            Assert.Equal("sch2", property.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName);
            Assert.Equal("tbl12", property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName);
        }

        [FactWithName]
        public void ComputedColumnAttribute()
        {
            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies.First().Value.GetProperty("compCol");

            Assert.Equal(DatabaseGeneratedOption.Computed, property.GetCustomAttribute<DatabaseGeneratedAttribute>().DatabaseGeneratedOption);
        }

        [FactWithName]
        public void Add1To1Relationship()
        {
            var variableName = $"tbl_{guid1}";
            var table = new Mock<Table>() { CallBase = true };
            table.Object.Database = "db";
            table.Object.Schema = "sch";
            table.Object.Name = "tbl";
            table.Setup(c => c.VariableName).Returns(variableName);
            table.Setup(c => c.Columns).Returns(new List<Column>
            {
                new Column{ColumnName = "pkCol", PKName = "pk1", FKName = "fk1", TableName=$"tbl1_{guid1}", ReferencedColumn="pkCol2", ReferencedTable=$"tbl2_{guid2}", ReferencedSchema="sch1", ReferencedDatabase="db1",  ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
            });
            table.Setup(c => c.GetHashCode()).Returns(123456);

            var table2 = new Mock<Table>() { CallBase = true };
            table2.Setup(c => c.VariableName).Returns($"db1_sch1_tbl2_{guid2}");
            table2.Setup(c => c.Columns).Returns(new List<Column>
            {
                new Column{ColumnName = "pkCol2", PKName = "pk2", ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
            });
            table2.Setup(c => c.GetHashCode()).Returns(456123);

            schema.Setup(c => c.Tables).Returns(new List<Table> { table.Object, table2.Object });

            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies[$"DYNAMICASSEMBLY.TBL_{guid1.ToString().ToUpper()}123456.REQTBL987654"].GetProperty($"tbl2_{guid2}pkColObject");
            var refProperty = classBuilder.ExistingAssemblies[$"DYNAMICASSEMBLY.DB1_SCH1_TBL2_{guid2.ToString().ToUpper()}456123.REQTBL987654"].GetProperty($"tbl1_{guid1}pkCol2Object");


            Assert.Equal($"Tbl2 {guid2}pk Col Object".ToUpper(), property.GetCustomAttribute<DisplayNameAttribute>().DisplayName.ToUpper());
             Assert.NotNull(property.GetCustomAttribute<DataMemberAttribute>());
             Assert.True(property.GetCustomAttribute<BrowsableAttribute>().Browsable);
            Assert.Equal("db1", property.GetCustomAttribute<ReferencedDbObjectAttribute>().DbName);
            Assert.Equal("sch1", property.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName);
            Assert.Equal($"tbl2_{guid2}", property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName);

            Assert.Equal($"Tbl1 {guid1}pk Col2 Object".ToUpper(), refProperty.GetCustomAttribute<DisplayNameAttribute>().DisplayName.ToUpper());
             Assert.NotNull(refProperty.GetCustomAttribute<DataMemberAttribute>());
             Assert.True(refProperty.GetCustomAttribute<BrowsableAttribute>().Browsable);
            Assert.Equal("db", refProperty.GetCustomAttribute<ReferencedDbObjectAttribute>().DbName);
            Assert.Equal("sch", refProperty.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName);
            Assert.Equal($"tbl", refProperty.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName);

        }

        [FactWithName]
        public void Add1ToManyRelationship()
        {
            var table = new Mock<Table>() { CallBase = true };
            table.Object.Database = "db";
            table.Object.Schema = "sch";
            table.Object.Name = $"tbl1_{guid1}";
            table.Setup(c => c.Columns).Returns(new List<Column>
            {
                new Column{ColumnName = "pkCol", PKName = "pk1", TableName=$"tbl1_{guid1}",   ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
            });
            table.Setup(c => c.GetHashCode()).Returns(123456);

            var table2 = new Mock<Table>() { CallBase = true };
            table2.Object.Database = "db";
            table2.Object.Schema = "sch";
            table2.Object.Name = $"tbl2_{guid2}";
            table2.Setup(c => c.Columns).Returns(new List<Column>
            {
                new Column{ColumnName = "pkCol2",  FKName = "fk1", TableName=$"tbl2_{guid2}", ReferencedColumn="pkCol", ReferencedTable=$"tbl1_{guid1}", ReferencedSchema="sch", ReferencedDatabase="db", ColumnType="int",  DataType = new DataType{SystemType = typeof(int) } },
            });
            table2.Setup(c => c.GetHashCode()).Returns(456123);

            schema.Setup(c => c.Tables).Returns(new List<Table> { table.Object, table2.Object });

            var classBuilder = new DynamicClassBuilder(schema.Object);
            classBuilder.CreateModelTypes("reqTbl");

            var property = classBuilder.ExistingAssemblies[$"DYNAMICASSEMBLY.DB_SCH_TBL1_{guid1.ToString().ToUpper()}123456.REQTBL987654"].GetProperty($"tbl2_{guid2}ListFrompkCol2");
            var refProperty = classBuilder.ExistingAssemblies[$"DYNAMICASSEMBLY.DB_SCH_TBL2_{guid2.ToString().ToUpper()}456123.REQTBL987654"].GetProperty($"tbl1_{guid1}pkCol2Object");


            Assert.Equal($"tbl1_{guid1}pkCol2Object", property.GetCustomAttribute<InversePropertyAttribute>().Property);
            Assert.Equal($"Tbl2 {guid2} List Frompk Col2".ToUpper(), property.GetCustomAttribute<DisplayNameAttribute>().DisplayName.ToUpper());
            Assert.NotNull(property.GetCustomAttribute<DataMemberAttribute>());
            Assert.True(property.GetCustomAttribute<BrowsableAttribute>().Browsable);
            Assert.Equal("db", property.GetCustomAttribute<ReferencedDbObjectAttribute>().DbName);
            Assert.Equal("sch", property.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName);
            Assert.Equal($"tbl2_{guid2}", property.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName);

            Assert.Equal($"Tbl1 {guid1}pk Col2 Object".ToUpper(), refProperty.GetCustomAttribute<DisplayNameAttribute>().DisplayName.ToUpper());
            Assert.NotNull(refProperty.GetCustomAttribute<DataMemberAttribute>());
            Assert.True(refProperty.GetCustomAttribute<BrowsableAttribute>().Browsable);
            Assert.Equal("db", refProperty.GetCustomAttribute<ReferencedDbObjectAttribute>().DbName);
            Assert.Equal("sch", refProperty.GetCustomAttribute<ReferencedDbObjectAttribute>().SchemaName);
            Assert.Equal($"tbl1_{guid1}", refProperty.GetCustomAttribute<ReferencedDbObjectAttribute>().TableName);

        }



        private void CreateAsm(string assemblyName, string tableName, bool createType = true)
        {
            AssemblyName an = new AssemblyName(assemblyName);

            var interfaces = Array.Empty<Type>();

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule." + tableName);
            TypeBuilder tb;

            tb = moduleBuilder.DefineType("DynamicType." + tableName
                                    , TypeAttributes.Public |
                                    TypeAttributes.Class |
                                    TypeAttributes.AutoClass |
                                    TypeAttributes.AnsiClass |
                                    TypeAttributes.BeforeFieldInit |
                                    TypeAttributes.AutoLayout | TypeAttributes.Sealed
                                    , typeof(PocoBase), interfaces);
            if (createType)
                tb.CreateType();


        }
    }
}
