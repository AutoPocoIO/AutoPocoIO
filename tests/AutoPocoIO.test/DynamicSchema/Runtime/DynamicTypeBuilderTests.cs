using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.test.DynamicSchema.Runtime
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DynamicTypeBuilderTests
    {
        [TestMethod]
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

            Assert.AreEqual(asmName + ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", typeBuilder.Assembly.FullName);
            Assert.AreEqual(1, typeBuilder.Module.GetTypes().Count());
            Assert.AreEqual("DynamicType.DB1_SCH1_TBL1", typeBuilder.Module.GetTypes()[0].FullName);
            Assert.AreEqual(typeof(PocoBase), typeBuilder.Module.GetTypes()[0].BaseType);
            Assert.AreEqual(TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed, typeBuilder.Module.GetTypes()[0].Attributes);
            Assert.IsTrue(typeBuilder.Module.GetTypes()[0].IsAutoLayout);
        }

        [TestMethod]
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
            Assert.AreEqual("get__prop1", property.GetGetMethod(false).Name);
            Assert.AreEqual(typeof(string), property.GetGetMethod(false).ReturnType);
            Assert.AreEqual(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetGetMethod(false).Attributes);
            Assert.AreEqual("set__prop1", property.GetSetMethod(false).Name);
            Assert.AreEqual(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetSetMethod(false).Attributes);
        }

        [TestMethod]
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
            Assert.AreEqual("get__prop1", property.GetGetMethod(false).Name);
            Assert.AreEqual(typeof(string), property.GetGetMethod(false).ReturnType);
            Assert.AreEqual(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetGetMethod(false).Attributes);
            Assert.AreEqual("set__prop1", property.GetSetMethod(false).Name);
            Assert.AreEqual(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.GetSetMethod(false).Attributes);
        }

        [TestMethod]
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
            Assert.AreEqual("get_prop11", property.GetGetMethod(false).Name);
            Assert.AreEqual(typeof(int), property.GetGetMethod(false).ReturnType);
            Assert.AreEqual(MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.Virtual, property.GetGetMethod(false).Attributes);
            Assert.AreEqual("set_prop11", property.GetSetMethod(false).Name);

        }
    }
}
