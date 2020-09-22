using AutoPocoIO.DynamicSchema.Models;
using System.Collections.Generic;

namespace AutoPocoIO.Dashboard.ViewModels
{
    internal class SchemaViewModel
    {
        public string ConnectorId { get; set; }
        public string ConnectorName { get; set; }
        public List<Table> Tables { get; set; }
        public List<View> Views { get; set; }
        public List<StoredProcedure> StoredProcedures { get; set; }
    }
}
