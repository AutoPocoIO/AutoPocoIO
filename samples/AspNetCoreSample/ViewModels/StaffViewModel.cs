namespace AspNetCoreSample.ViewModels
{
    public class StaffViewModel
    {
        public int Staff_id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Name => $"{Last_Name}, {First_Name}";
    }
}
