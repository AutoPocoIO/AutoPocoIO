using AutoPocoIO.CustomAttributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    partial class DbAdapterTests
    {
        [DatabaseName("Db1")]
        [Table("tbla", Schema ="dbo")]
        private class _dbo_tbla
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [DatabaseName("Db1")]
        [Table("tblb", Schema = "dbo")]
        private class _dbo_tblb
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [DatabaseName("Db1")]
        [Table("tblc", Schema = "dbo")]
        private class _dbo_tblc
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
