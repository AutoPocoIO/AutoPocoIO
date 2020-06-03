using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPocoIO.Dashboard.ViewModels
{
    internal class NavigationPropertyViewModel
    {
        public string Relationship { get; internal set; }
        public string Name { get; internal set; }
        public string Type { get; internal set; }
        public string ReferencedTable { get; internal set; }
        public string ReferencedSchema { get; internal set; }
    }
}
