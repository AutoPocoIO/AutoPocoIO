namespace AutoPocoIO.DynamicSchema.Models
{
    public class DBParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsOutput { get; set; }
        public bool IsNullable { get; set; }
    }
}
