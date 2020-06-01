using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using Xunit;
using System;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.test.DynamicSchema.Runtime
{
    
     [Trait("Category", TestCategories.Unit)]
    public class DynamicTypeBuilderTests
    {
        [FactWithName]
        public void GetTypeBuilder()
        {
            Table table = new Table
            {
                Name = "tbl1",
                Schema = "sch1",
                Database = "db1"
            };

            var asmName = "testAsm" + Guid.NewGuid().ToString();
            var typeBuilder = DynamicTypeBuilder.GetTypeBuilder(table, typeof(PocoBase), asmName);

            Assert.Equal(asmName + ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", typeBuilder.Assembly.FullName);
            Assert.Single(typeBuilder.Module.GetTypes());
            Assert.Equal("DynamicType.DB1_SCH1_TBL1", typeBuilder.Module.GetTypes()[0].FullName);
            Assert.Equal(typeof(PocoBase), typeBuilder.Module.GetTypes()[0].BaseType);
            Assert.Equal(TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed, typeBuilder.Module.GetTypes()[0].Attributes);
             Assert.True(typeBuilder.Module.GetTypes()[0].IsAutoLayout);
        }

        [FactWithName]
        public void CreatePropertyWithNotify()
        {
            Table table = new Table
            {
                Name = "tbl1",
                Schema = "sch1",
                Database = "db1"
            };

            var asmName = "testAsm" + Guid.NewGuid().ToString();

            var typeBuilder = DynamicTypeBuilder.GetTypeBuilder(table, typeof(PocoBase), asmName);

            var property = DynamicTypeBuilder.CreateProperty(typeBuilder, "prop1", typeof(string), true);
            Assert.Equal("get__prop1", property.GetGetMethod(false).Name);
            Assert.Equal(typeof(string), property.GetGetMethod(false).ReturnType);
            Assert.Equal(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetGetMethod(false).Attributes);
            Assert.Equal("set__prop1", property.GetSetMethod(false).Name);
            Assert.Equal(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetSetMethod(false).Attributes);
        }

        [FactWithName]
        public void CreatePropertyNoNotify()
        {
            Table table = new Table
            {
                Name = "tbl1",
                Schema = "sch1",
                Database = "db1"
            };

            var asmName = "testAsm" + Guid.NewGuid().ToString();

            var typeBuilder = DynamicTypeBuilder.GetTypeBuilder(table, typeof(PocoBase), asmName);

            var property = DynamicTypeBuilder.CreateProperty(typeBuilder, "prop1", typeof(string), false);
            Assert.Equal("get__prop1", property.GetGetMethod(false).Name);
            Assert.Equal(typeof(string), property.GetGetMethod(false).ReturnType);
            Assert.Equal(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetGetMethod(false).Attributes);
            Assert.Equal("set__prop1", property.GetSetMethod(false).Name);
            Assert.Equal(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetSetMethod(false).Attributes);
        }

        [FactWithName]
        public void CreatePropertyVirtual()
        {
            Table table = new Table
            {
                Name = "tbl1",
                Schema = "sch1",
                Database = "db1"
            };

            var asmName = "testAsm" + Guid.NewGuid().ToString();

            var typeBuilder = DynamicTypeBuilder.GetTypeBuilder(table, typeof(PocoBase), asmName);

            var property = DynamicTypeBuilder.CreateVirtualProperty(typeBuilder, "prop11", typeof(int));
            Assert.Equal("get_prop11", property.GetGetMethod(false).Name);
            Assert.Equal(typeof(int), property.GetGetMethod(false).ReturnType);
            Assert.Equal(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.Virtual, property.GetGetMethod(false).Attributes);
            Assert.Equal("set_prop11", property.GetSetMethod(false).Name);

        }
    }
}
