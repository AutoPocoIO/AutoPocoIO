namespace AutoPocoIO.Models
{
    /// <summary>
    /// Flatten User Join information
    /// </summary>
    public class UserJoinConfiguration
    {
        public string Alias { get; set; }
        public string PrincipalDatabase { get; set; }
        public string PrincipalSchema { get; set; }
        public string PrincipalTable { get; set; }
        public string PrincipalColumns { get; set; }
        public string DependentDatabase { get; set; }
        public string DependentSchema { get; set; }
        public string DependentTable { get; set; }
        public string DependentColumns { get; set; }
    }
}
