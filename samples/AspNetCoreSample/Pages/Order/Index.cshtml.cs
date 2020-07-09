using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCoreSample.Pages.Order
{
    public class IndexModel : PageModel
    {
        private readonly ITableOperations _tableOperations;

        public IndexModel(ITableOperations tableOperations)
        {
            _tableOperations = tableOperations;
        }

        public OrderDetailViewModel item { get; set; }
        public void OnGet(int orderId)
        {
            //Not logged example because logger is not passed to operation
            item = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", orderId.ToString());
        }
    }
}