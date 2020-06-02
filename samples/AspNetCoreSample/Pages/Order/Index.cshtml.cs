using AspNetCoreSample.ViewModels;
using AutoPocoIO.Api;
using AutoPocoIO.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCoreSample.Pages.Order
{
    public class IndexModel : PageModel
    {
        private readonly ITableOperations _tableOperations;
        private IStoredProcedureOperations ops;
        private readonly ILoggingService _loggingService;

        public IndexModel(ITableOperations tableOperations, IStoredProcedureOperations op, ILoggingService loggingService)
        {
            _tableOperations = tableOperations;
            _loggingService = loggingService;
            ops = op;
        }

        public OrderDetailViewModel item { get; set; }
        public void OnGet(int orderId)
        {
            //Not logged example because logger is not passed to operation
            //item = _tableOperations.GetById<OrderDetailViewModel>("sampleSales", "orders", orderId.ToString());

            var test = ops.ExecuteNoParameters("appdb", "GetUserRoles");
        }
    }
}