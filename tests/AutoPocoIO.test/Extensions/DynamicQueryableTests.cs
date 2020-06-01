using System.Collections.Generic;
using System.Linq.AutoPoco;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    public class SinglePropertyClass { public int Id { get; set; } }
    public class NestedObjectPropertyClass { public int Id2 { get; set; } public SinglePropertyClass OtherProp { get; set; } }
    public class NestedListPropertyClass { public int Id2 { get; set; } public IEnumerable<SinglePropertyClass> OtherProp { get; set; } }
    public class NestedIntListPropertyClass { public int Id2 { get; set; } public IEnumerable<int> OtherProp { get; set; } }
    
    [Trait("Category", TestCategories.Unit)]
    public class DynamicQueryableTests
    {


       [FactWithName]
        public void AddPropertySingleProperty()
        {
            string select = DynamicQueryable.AddProperties(typeof(SinglePropertyClass), typeof(SinglePropertyClass), new List<object>());
            Assert.Equal("new AutoPocoIO.test.Extensions.SinglePropertyClass(Id as Id)", select);
        }

        [FactWithName]
        public void AddPropertySinglePropertyWithCast()
        {
            var annon = new { Id = (int?)1 };
            string select = DynamicQueryable.AddProperties(annon.GetType(), typeof(SinglePropertyClass), new List<object>());
            Assert.Equal("new AutoPocoIO.test.Extensions.SinglePropertyClass(Int32(Id == null ? @0 : Id) as Id)", select);
        }


        [FactWithName]
        public void AddPropertyNullSingleProperty()
        {
            var annon = new { };
            string select = DynamicQueryable.AddProperties(annon.GetType(), typeof(SinglePropertyClass), new List<object>());
            Assert.Equal("new AutoPocoIO.test.Extensions.SinglePropertyClass()", select);

        }

        [FactWithName]
        public void AddPropertyObjectProperty()
        {
            string select = DynamicQueryable.AddProperties(typeof(NestedObjectPropertyClass), typeof(NestedObjectPropertyClass), new List<object>());
            Assert.Equal("new AutoPocoIO.test.Extensions.NestedObjectPropertyClass(Id2 as Id2, new AutoPocoIO.test.Extensions.SinglePropertyClass(OtherProp.Id as Id) as OtherProp)", select);
        }

        [FactWithName]
        public void AddPropertyNoMatches()
        {
            string select = DynamicQueryable.AddProperties(typeof(NestedObjectPropertyClass), typeof(SinglePropertyClass), new List<object>());
            Assert.Equal("new AutoPocoIO.test.Extensions.SinglePropertyClass()", select);
        }

        [FactWithName]
        public void AddPropertyListProperty()
        {
            string select = DynamicQueryable.AddProperties(typeof(NestedListPropertyClass), typeof(NestedListPropertyClass), new List<object>());
            Assert.Equal("new AutoPocoIO.test.Extensions.NestedListPropertyClass(Id2 as Id2, OtherProp.Select(new AutoPocoIO.test.Extensions.SinglePropertyClass(Id as Id)) as OtherProp)", select);
        }

        [FactWithName]
        public void AddPropertyIntListProperty()
        {
            string select = DynamicQueryable.AddProperties(typeof(NestedIntListPropertyClass), typeof(NestedIntListPropertyClass), new List<object>());
            Assert.Equal("new AutoPocoIO.test.Extensions.NestedIntListPropertyClass(Id2 as Id2, OtherProp as OtherProp)", select);
        }
    }
}
