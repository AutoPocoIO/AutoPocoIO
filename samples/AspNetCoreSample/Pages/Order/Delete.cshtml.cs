using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCoreSample.Pages.Order
{
    public class DeleteModel : PageModel
    {
        private readonly ITableOperations _tableOperations;
        private readonly ILoggingService _loggingService;

        public DeleteModel(ITableOperations tableOperations, ILoggingService loggingService)
        {
            _tableOperations = tableOperations;
            _loggingService = loggingService;
        }

        public OrderDetailViewModel item { get; set; }

        public void OnGet(int orderId)
        {
            //Not logged example because logger is not passed to operation
            item = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", orderId);
        }

        public IActionResult OnPost(int orderId)
        {
            _tableOperations.DeleteRow("sampleSales", "orders", orderId.ToString(), _loggingService);
            return RedirectToPage("./Index");
        }
    }
}