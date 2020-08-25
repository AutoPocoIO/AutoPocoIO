using System.Collections.Generic;
using System.Web.Mvc;

namespace AspNetSample.Models
{
    public class OrderDetailsPageModel
    {
        public OrderDetailViewModel item { get; set; }

        public IEnumerable<SelectListItem> Stores { get; set; }
        public IEnumerable<SelectListItem> Customers { get; set; }
        public IEnumerable<SelectListItem> SalesReps { get; set; }
    }
}
