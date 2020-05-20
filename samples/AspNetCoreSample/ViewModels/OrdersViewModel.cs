namespace AspNetCoreSample.ViewModels
{
    public class OrdersViewModel
    {
        public int Order_Id { get; set; }
        public int Customer_Id { get; set; }
        public byte Order_Status { get; set; }
        public CustomerViewModel Customer { get; set; }
    }

    public class CustomerViewModel
    {
        public int Customer_Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
    }
}
