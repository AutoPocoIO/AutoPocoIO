using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.Sample.AspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITableOperations _tableOperations;
        private readonly ILoggingService _loggingService;
        public IndexModel(ITableOperations tableOperations, ILoggingService loggingService)
        {
            _tableOperations = tableOperations;
            _loggingService =  loggingService;
        }

        public IEnumerable<OrdersViewModel> Orders { get; private set; }


        public void OnGet()
        {
            try
            {
                Orders = _tableOperations.GetAll<OrdersViewModel>("sampleSales", "orders", _loggingService);
            }
            catch (ConnectorNotFoundException)
            {
                ViewData["ConnectorNeeded"] = "Connector sampleSales not set up or disabled.  Go to the dashboard to configure.";
                Orders = Array.Empty<OrdersViewModel>();
            }
        }
    }
}
