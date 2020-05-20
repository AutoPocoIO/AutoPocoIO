using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using AutoPocoIO.Exceptions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace AutoPocoIO.Sample.AspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITableOperations _tableOperations;

        public IndexModel(ITableOperations tableOperations)
        {
            _tableOperations = tableOperations;
        }

        public IEnumerable<OrdersViewModel> Orders { get; private set; }


        public void OnGet()
        {
            try
            {
                Orders = _tableOperations.GetAll<OrdersViewModel>("sampleSales", "orders");
            }
            catch (ConnectorNotFoundException)
            {
                ViewData["ConnectorNeeded"] = "Connector sampleSales not set up.  Go to the dashboard to set configure.";
                Orders = Array.Empty<OrdersViewModel>();
            }
        }
    }
}
