using AutoPocoIO.DynamicSchema.Models;
using AutoPocoIO.DynamicSchema.Util;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace AutoPocoIO.test.DynamicSchema.Util
{
    [Trait("Category", TestCategories.Unit)]
    public class UtilsTests
    {
        [FactWithName]
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
            Assert.Equal("DYNAMICASSEMBLY.DB_SCH_TBL1234.PARENT78945", actualValue);
        }

        [FactWithName]
        public void FindLoadedAsmWithNoTypes()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] { typeof(object) }, null));
            void act() => AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);
            Assert.Throws<InvalidOperationException>(act);
        }

        [FactWithName]
        public void FindLoadAsmGetTypeReturnsNull()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Returns((Type[])null);
             void act() => AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);
            Assert.Throws<InvalidOperationException>(act);
        }

        [FactWithName]
        public void FindLoadAsmGetTypesReturnsNoRecords()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Returns(Array.Empty<Type>);
            void act() => AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);
            Assert.Throws<InvalidOperationException>(act);
        }

        [FactWithName]
        public void FindLoadAsmGetTypesReturnsType()
        {
            var asmMoq = new Mock<Asm>();
            asmMoq.Setup(c => c.GetTypes()).Returns(new Type[] { typeof(object) });
            var type = AutoPocoIO.DynamicSchema.Util.Utils.FindLoadedAssembly(new List<Assembly> { asmMoq.Object }, true);

            Assert.Equal(typeof(object), type);
        }

        [FactWithName]
        public void FancyLabelStartWithCapLetter()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("thisisalllowercase");
            Assert.Equal("Thisisalllowercase", actual);
        }

        [FactWithName]
        public void FancyLabelTitleCaseWhenIncludingSpaces()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("this is all lowercase");
            Assert.Equal("This Is All Lowercase", actual);
        }

        [FactWithName]
        public void FancyLabelTitleCaseWhenIncludingUnderScore()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("this_is_all_lowercase");
            Assert.Equal("This Is All Lowercase", actual);
        }

        [FactWithName]
        public void FancyLabelTrimTrailingUnderscore()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("lowercase_");
            Assert.Equal("Lowercase", actual);
        }

        [FactWithName]
        public void FancyLabelTrailingCapsTogether()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("lowercaSE");
            Assert.Equal("Lowerca SE", actual);
        }

        [FactWithName]
        public void FancyLabelTrailingSingleCap()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("lowercasE");
            Assert.Equal("Lowercas E", actual);
        }

        [FactWithName]
        public void FancyLabelKeepConsecutiveCapsTogeteher()
        {
            string actual = AutoPocoIO.DynamicSchema.Util.Utils.GetFancyLabel("loWERcase");
            Assert.Equal("Lo WE Rcase", actual);
        }

        [FactWithName]
        public void ColumnToPropertyName()
        {
            var col = new Column { ColumnName = "colName" };
            var result = col.ColumnToPropertyName();
            Assert.Equal("colName", result);
        }

        [FactWithName]
        public void ColumnToPropertyNameReplaceDotWithUnderScore()
        {
            var col = new Column { ColumnName = "col.Name" };
            var result = col.ColumnToPropertyName();
            Assert.Equal("col_Name", result);
        }

        [FactWithName]
        public void ColumnToPropertyAddCIfStartsWithANumber()
        {
            var col = new Column { ColumnName = "1colName" };
            var result = col.ColumnToPropertyName();
            Assert.Equal("C1colName", result);
        }

        public class Asm : Assembly
        {
            public Asm() : base() { }
        }
    }
}
