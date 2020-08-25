using AspNetCoreSample.ViewModels;
using AspNetSample.Models;
using AutoPocoIO.Api;
using AutoPocoIO.Services;
using System.Linq;
using System.Web.Mvc;

namespace AspNetSample.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ITableOperations _tableOperations;
        private readonly ILoggingService _loggingService;

        public OrdersController(ITableOperations tableOperations, ILoggingService loggingService)
        {
            _tableOperations = tableOperations;
            _loggingService = loggingService;
        }

        [Route("Orders/{id}")]
        public ActionResult Index(int id)
        {
            var model = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", id);
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var model = new OrderDetailsPageModel
            {
                item = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", id),
                Stores = _tableOperations.GetAll<StoreViewModel>("sampleSales", "stores")
                                         .OrderBy(c => c.store_name)
                                         .Select(c => new SelectListItem { Text = c.store_name, Value = c.Store_Id.ToString() }),
                Customers = _tableOperations.GetAll<CustomerViewModel>("sampleSales", "customers")
                                            .OrderBy(c => c.Name)
                                            .Select(c => new SelectListItem { Text = c.Name, Value = c.Customer_Id.ToString() }),
                SalesReps = _tableOperations.GetAll<StaffViewModel>("sampleSales", "staffs")
                                           .OrderBy(c => c.Name)
                                           .Select(c => new SelectListItem { Text = c.Name, Value = c.Staff_id.ToString() })
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(OrderDetailsPageModel model)
        {
            _tableOperations.UpdateRow("sampleSales", "orders", model.item, _loggingService);
            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpGet]
        [Route("Orders/Delete/{id}")]
        public ActionResult Delete(int id)
        {
            //Not logged example because logger is not passed to operation
            var model = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", id);
            return View(model);
        }

        [HttpPost]
        [Route("Orders/Delete/{id}")]
        public ActionResult DeletePost(int id)
        {
            _tableOperations.DeleteRow("sampleSales", "orders", _loggingService, id);
            return RedirectToAction(nameof(Index), "Home");
        }
    }
}