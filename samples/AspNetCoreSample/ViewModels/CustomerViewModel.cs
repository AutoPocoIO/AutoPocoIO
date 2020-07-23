namespace AspNetCoreSample.ViewModels
{
    public class CustomerViewModel
    {
        public int Customer_Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Name => $"{Last_Name}, {First_Name}";
    }
}
