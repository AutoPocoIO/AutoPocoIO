using AutoPocoIO.DynamicSchema.Models;
using Xunit;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    
     [Trait("Category", TestCategories.Unit)]
    public class VariableNameTests
    {
        [FactWithName]
        public void TableStringIsVariableName()
        {
            var tbl = new Table { Database = "db1", Schema = "sch1", Name = "tbl1" };

            Assert.Equal("db1_sch1_tbl1", tbl.ToString());
            Assert.Equal(tbl.VariableName, tbl.ToString());
        }
    }
}
