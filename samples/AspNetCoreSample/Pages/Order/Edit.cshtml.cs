using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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


        public void OnGet(int orderId)
        {
            //Not logged example because logger is not passed to operation
            item = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", orderId.ToString());
        }

        public  IActionResult OnPost()
        {
            _tableOperations.UpdateRow("sampleSales", "orders", item, _loggingService);
            return RedirectToPage("../Index");
        }
    }
}