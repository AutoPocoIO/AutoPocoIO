using AspNetCoreSample.ViewModels;
using AspNetSample.Models;
using AutoPocoIO.Api;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace AspNetSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITableOperations _tableOperations;
        private readonly ILoggingService _loggingService;
        public HomeController(ITableOperations tableOperations, ILoggingService loggingService)
        {
            _tableOperations = tableOperations;
            _loggingService = loggingService;
        }

        public ActionResult Index(int currentpage = 1)
        {
            var model = new HomePageModel { CurrentPage = currentpage };
            try
            {
                var allOrders = _tableOperations.GetAll<OrdersViewModel>("sampleSales", "orders", _loggingService);
                model.Count = allOrders.Count();

                model.Orders = allOrders.OrderBy(d => d.Order_Id)
                    .Skip((model.CurrentPage - 1) * model.PageSize)
                    .Take(model.PageSize);


            }
            catch (ConnectorNotFoundException)
            {
                ViewData["ConnectorNeeded"] = "Connector sampleSales not set up or disabled.  Go to the dashboard to configure.";
                model.Orders = Array.Empty<OrdersViewModel>();
            }

            return View(model);
        }
    }
}