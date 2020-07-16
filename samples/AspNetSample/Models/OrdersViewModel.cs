namespace AspNetCoreSample.ViewModels
{
    public class OrdersViewModel
    {
        public int Order_Id { get; set; }
        public int Customer_Id { get; set; }
        public byte Order_Status { get; set; }
        public string Name => $"{CustomersObjectFromCustomer_id.Last_Name}, {CustomersObjectFromCustomer_id.First_Name}";
        public CustomerViewModel CustomersObjectFromCustomer_id { get; set; }
    }

    public class CustomerViewModel
    {
        public int Customer_Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
    }
}
