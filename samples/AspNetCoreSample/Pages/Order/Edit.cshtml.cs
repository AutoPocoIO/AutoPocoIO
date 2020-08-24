using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCoreSample.Pages.Order
{
    public class EditModel : PageModel
    {
        private readonly ITableOperations _tableOperations;
        private readonly ILoggingService _loggingService;

        public EditModel(ITableOperations tableOperations, ILoggingService loggingService)
        {
            _tableOperations = tableOperations;
            _loggingService = loggingService;
        }

        [BindProperty]
        public OrderDetailViewModel item { get; set; }

        public IEnumerable<SelectListItem> Stores { get; set; }
        public IEnumerable<SelectListItem> Customers { get; set; }
        public IEnumerable<SelectListItem> SalesReps { get; set; }

        public void OnGet(int orderId)
        {
            //Not logged example because logger is not passed to operation
            item = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", orderId);
            Stores = _tableOperations.GetAll<StoreViewModel>("sampleSales", "stores")
                                     .OrderBy(c => c.store_name)
                                     .Select(c => new SelectListItem { Text = c.store_name, Value = c.Store_Id.ToString() });
            Customers = _tableOperations.GetAll<CustomerViewModel>("sampleSales", "customers")
                                        .OrderBy(c => c.Name)
                                        .Select(c => new SelectListItem { Text = c.Name, Value = c.Customer_Id.ToString() });
            SalesReps = _tableOperations.GetAll<StaffViewModel>("sampleSales", "staffs")
                                       .OrderBy(c => c.Name)
                                       .Select(c => new SelectListItem { Text = c.Name, Value = c.Staff_id.ToString() });
        }

        public IActionResult OnPost()
        {
            _tableOperations.UpdateRow("sampleSales", "orders", item, _loggingService);
            return RedirectToPage("../Index");
        }
    }
}