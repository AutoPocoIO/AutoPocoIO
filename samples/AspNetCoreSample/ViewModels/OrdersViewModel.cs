namespace AspNetCoreSample.ViewModels
{
    public class OrdersViewModel
    {
        public int Order_Id { get; set; }
        public int Customer_Id { get; set; }
        public byte Order_Status { get; set; }
        public string Name => CustomersObjectFromCustomer_id.Name;
        public CustomerViewModel CustomersObjectFromCustomer_id { get; set; }
    }
}
