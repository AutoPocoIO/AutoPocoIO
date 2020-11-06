namespace AutoPocoIO.Models
{
    public class NavigationPropertyDefinition
    {
        public string Relationship { get; set; }
        public string Name { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedSchema { get; set; }
        public string FromProperty { get; set; }
        public string ToProperty { get; set; }
        public bool IsUserDefinied { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
