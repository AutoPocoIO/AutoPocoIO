using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoPocoIO.Sample.AspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITableOperations _tableOperations;
        private readonly ILoggingService _loggingService;
        public IndexModel(ITableOperations tableOperations, ILoggingService loggingService)
        {
            _tableOperations = tableOperations;
            _loggingService = loggingService;
        }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int Count { get; set; }
        public int PageSize { get; set; } = 10;
        public IEnumerable<OrdersViewModel> Orders { get; private set; }
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(Count, PageSize));

        public void OnGet()
        {
            try
            {
                var allOrders = _tableOperations.GetAll<OrdersViewModel>("sampleSales", "orders", _loggingService);
                Count = allOrders.Count();

                Orders = allOrders.OrderBy(d => d.Order_Id)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize);


            }
            catch (ConnectorNotFoundException)
            {
                ViewData["ConnectorNeeded"] = "Connector sampleSales not set up or disabled.  Go to the dashboard to configure.";
                Orders = Array.Empty<OrdersViewModel>();
            }
        }
    }
}
