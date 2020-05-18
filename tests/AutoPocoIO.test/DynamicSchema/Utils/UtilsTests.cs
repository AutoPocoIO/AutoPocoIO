using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoPocoIO.test.DynamicSchema.Util
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class UtilsTests
    {
        [TestMethod]
        public void DynamicAssemblyNameFormat()
        {
            Config config = new Config()
            {
                DatabaseConnectorName = "name",
            };

            var tableMoq = new Mock<Table>();
            tableMoq.Setup(c => c.GetHashCode()).Returns(1234);
            tableMoq.SetupGet(c => c.VariableName).Returns("db_sch_tbl");

            string actualValue = AutoPocoIO.DynamicSchema.Util.Utils.AssemblyName(tableMoq.Object, "parent", 78945);
            Assert.AreEqual("DYNAMICASSEMBLY.DB_SCH_TBL1234.PARENT78945", actualValue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FindLoadedAsmWithNoTypes()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] { typeof(object) }, null));
            AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FindLoadAsmGetTypeReturnsNull()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Returns((Type[])null);
            AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FindLoadAsmGetTypesReturnsNoRecords()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Returns(Array.Empty<Type>);
            AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);
        }

        [TestMethod]
        public void FindLoadAsmGetTypesReturnsType()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Returns(new Type[] { typeof(object) });
            var type = AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);

            Assert.AreEqual(typeof(object), type);
        }

        [TestMethod]
        public void FancyLabelStartWithCapLetter()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("thisisalllowercase");
            Assert.AreEqual("Thisisalllowercase", actual);
        }

        [TestMethod]
        public void FancyLabelTitleCaseWhenIncludingSpaces()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("this is all lowercase");
            Assert.AreEqual("This Is All Lowercase", actual);
        }

        [TestMethod]
        public void FancyLabelTitleCaseWhenIncludingUnderScore()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("this_is_all_lowercase");
            Assert.AreEqual("This Is All Lowercase", actual);
        }

        [TestMethod]
        public void FancyLabelTrimTrailingUnderscore()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("lowercase_");
            Assert.AreEqual("Lowercase", actual);
        }

        [TestMethod]
        public void FancyLabelTrailingCapsTogether()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("lowercaSE");
            Assert.AreEqual("Lowerca SE", actual);
        }

        [TestMethod]
        public void FancyLabelTrailingSingleCap()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("lowercasE");
            Assert.AreEqual("Lowercas E", actual);
        }

        [TestMethod]
        public void FancyLabelKeepConsecutiveCapsTogeteher()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("loWERcase");
            Assert.AreEqual("Lo WE Rcase", actual);
        }

        [TestMethod]
        public void ColumnToPropertyName()
        {
            var col = new Column { ColumnName = "colName" };
            var result = col.ColumnToPropertyName();
            Assert.AreEqual("colName", result);
        }

        [TestMethod]
        public void ColumnToPropertyNameReplaceDotWithUnderScore()
        {
            var col = new Column { ColumnName = "col.Name" };
            var result = col.ColumnToPropertyName();
            Assert.AreEqual("col_Name", result);
        }

        [TestMethod]
        public void ColumnToPropertyAddCIfStartsWithANumber()
        {
            var col = new Column { ColumnName = "1colName" };
            var result = col.ColumnToPropertyName();
            Assert.AreEqual("C1colName", result);
        }

        public class Asm : Assembly
        {
            public Asm() : base() { }
        }
    }
}
