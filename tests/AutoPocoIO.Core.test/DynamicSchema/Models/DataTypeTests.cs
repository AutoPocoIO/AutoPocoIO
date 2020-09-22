using AutoPocoIO.DynamicSchema.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DataTypeTests
    {
        [TestMethod]
        public void StringValue()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            Assert.AreEqual("(dbtype) String", obj.ToString());
        }

        [TestMethod]
        public void EqualsDifferentTypesReturnsFalse()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var result = obj.Equals("dbtype");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EqualsCastToDataType()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            object obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            var result = obj.Equals(obj2);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HashCodeIsValuebased()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            Assert.AreEqual(obj2.GetHashCode(), obj.GetHashCode());
        }


        [TestMethod]
        public void EqualsSymbol()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            var result = obj == obj2;

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void NotEqualsSymbolComparseDbtype()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype123", SystemType = typeof(string) };

            var result = obj != obj2;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void NotEqualsSymbolComparseSystemType()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(int) };

            var result = obj != obj2;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EqualsComparseDbtype()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype123", SystemType = typeof(string) };

            var result = obj.Equals(obj2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EqualsComparseSystemType()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(int) };

            var result = obj.Equals(obj2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EqualsComparseSystemTypeTrueIfBothMatch()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            var result = obj.Equals(obj2);

            Assert.IsTrue(result);
        }

    }
}
