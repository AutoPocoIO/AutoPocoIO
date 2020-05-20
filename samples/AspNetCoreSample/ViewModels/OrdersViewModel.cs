namespace AspNetCoreSample.ViewModels
{
    public class OrdersViewModel
    {
        public int? Order_Id { get; set; }
        public int? Customer_Id { get; set; }
        public byte? Order_Status { get; set; }
        public string Name => $"{customerscustomer_idObject.Last_Name}, {customerscustomer_idObject.First_Name}";
        public CustomerViewModel customerscustomer_idObject { get; set; }
    }

    public class CustomerViewModel
    {
        public int? Customer_Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
    }
}
