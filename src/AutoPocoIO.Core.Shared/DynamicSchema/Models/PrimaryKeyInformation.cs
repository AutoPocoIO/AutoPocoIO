using System;

namespace AutoPocoIO.DynamicSchema.Models
{
    public class PrimaryKeyInformation
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
