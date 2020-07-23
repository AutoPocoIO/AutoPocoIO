using System.Collections.Generic;

namespace AspNetCoreSample.ViewModels
{
    public class OrderDetailViewModel
    {
        public int Order_Id { get; set; }
        public byte Order_Status { get; set; }
        public int Customer_Id { get; set; }
        public int Store_Id { get; set; }
        public int Staff_Id { get; set; }

        public StoreViewModel StoresObjectFromStore_Id { get; set; }
        public StaffViewModel StaffsObjectFromStaff_id { get; set; }
        public CustomerViewModel CustomersObjectFromCustomer_id { get; set; }
        public IEnumerable<OrderItemsViewModel> Order_itemsListFromOrder_id { get; set; }
    }

 



    public class OrderItemsViewModel
    {
        public int Order_Id { get; set; }
        public int Item_Id { get; set; }
        public int Product_Id { get; set; }
        public int Quantity { get; set; }
        public decimal List_price { get; set; }
        public decimal Discount { get; set; }
        public decimal RetailPrice => (List_price * (1 - Discount));
        public decimal ItemTotal => RetailPrice * Quantity;

        public ProductViewModel ProductsObjectFromProduct_Id { get; set; }
    }

    public class ProductViewModel
    {
        public int Product_Id { get; set; }
        public string Product_Name { get; set; }
    }

}
