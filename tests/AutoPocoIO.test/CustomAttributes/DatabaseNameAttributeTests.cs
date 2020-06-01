using AutoPocoIO.CustomAttributes;
using Xunit;

namespace AutoPocoIO.test.CustomAttributes
{
    
     [Trait("Category", TestCategories.Unit)]
    public class DatabaseNameAttributeTests
    {
        [FactWithName]
        public void ConstructorSetsDatabaseName()
        {
            var attr = new DatabaseNameAttribute("test123");
            Assert.Equal("test123", attr.DatabaseName);
        }
    }
}
