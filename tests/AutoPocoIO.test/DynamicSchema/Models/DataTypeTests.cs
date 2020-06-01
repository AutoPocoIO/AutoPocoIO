using AutoPocoIO.DynamicSchema.Models;
using Xunit;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    
     [Trait("Category", TestCategories.Unit)]
    public class DataTypeTests
    {
        [FactWithName]
        public void StringValue()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            Assert.Equal("(dbtype) String", obj.ToString());
        }

        [FactWithName]
        public void EqualsDifferentTypesReturnsFalse()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var result = obj.Equals("dbtype");

             Assert.False(result);
        }

        [FactWithName]
        public void EqualsCastToDataType()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            object obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            var result = obj.Equals(obj2);

             Assert.True(result);
        }

        [FactWithName]
        public void HashCodeIsValuebased()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            Assert.Equal(obj2.GetHashCode(), obj.GetHashCode());
        }


        [FactWithName]
        public void EqualsSymbol()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            var result = obj == obj2;

             Assert.True(result);
        }


        [FactWithName]
        public void NotEqualsSymbolComparseDbtype()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype123", SystemType = typeof(string) };

            var result = obj != obj2;

             Assert.True(result);
        }

        [FactWithName]
        public void NotEqualsSymbolComparseSystemType()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(int) };

            var result = obj != obj2;

             Assert.True(result);
        }

        [FactWithName]
        public void EqualsComparseDbtype()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype123", SystemType = typeof(string) };

            var result = obj.Equals(obj2);

             Assert.False(result);
        }

        [FactWithName]
        public void EqualsComparseSystemType()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(int) };

            var result = obj.Equals(obj2);

             Assert.False(result);
        }

        [FactWithName]
        public void EqualsComparseSystemTypeTrueIfBothMatch()
        {
            var obj = new DataType { DbType = "dbtype", SystemType = typeof(string) };
            var obj2 = new DataType { DbType = "dbtype", SystemType = typeof(string) };

            var result = obj.Equals(obj2);

             Assert.True(result);
        }

    }
}
