using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Models
{

    public class StoredProcedure
    {
        public StoredProcedure()
        {
            Parameters = new List<DBParameter>();
        }
        public string Name { get; set; }
        public List<DBParameter> Parameters { get; }
        public string Schema { get; set; }
        public string Database { get; set; }
    }
}
