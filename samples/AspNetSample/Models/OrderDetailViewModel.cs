using System.Collections.Generic;

namespace AspNetCoreSample.ViewModels
{
    public class OrderDetailViewModel
    {
        public int Order_Id { get; set; }
        public int Customer_Id { get; set; }
        public int Store_Id { get; set; }
        public int Staff_Id { get; set; }

        public StoreViewModel StoresObjectFromStore_Id { get; set; }
        public IEnumerable<OrderItemsViewModel> Order_itemsListFromOrder_id { get; set; }
    }

    public class StoreViewModel
    {
        public int Store_Id { get; set; }
        public string store_name { get; set; }
    }

    public class OrderItemsViewModel
    {
        public int Order_Id { get; set; }
        public int Item_Id { get; set; }
        public int Product_Id { get; set; }
        public ProductViewModel ProductsObjectFromProduct_Id { get; set; }
    }

    public class ProductViewModel
    {
        public int Product_Id { get; set; }
        public string Product_Name { get; set; }
    }

}
