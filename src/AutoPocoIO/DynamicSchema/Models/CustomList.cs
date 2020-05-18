using System.Collections.Generic;

namespace AutoPocoIO.DynamicSchema.Models
{
    internal class CustomList<T> : List<T>
    {
        public IEnumerable<dynamic> AsDynamic()
        {
            foreach (var obj in this) yield return obj;
        }
    }
}
