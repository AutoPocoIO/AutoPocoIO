using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreSample.ViewModels
{
    public class OrderDetailViewModel
    {
        public int Order_Id { get; set; }
        public int Customer_Id { get; set; }
        public int Store_Id { get; set; }
        public int Staff_Id { get; set; }

        public StoreViewModel storesStore_IdObject { get; set; }
        public IEnumerable<OrderItemsViewModel> order_itemsListFromorder_id { get; set; }
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
        public ProductViewModel productsProduct_IdObject { get; set; }
    }

    public class ProductViewModel
    {
        public int Product_Id { get; set; }
        public string Product_Name { get; set; }
    }

}
