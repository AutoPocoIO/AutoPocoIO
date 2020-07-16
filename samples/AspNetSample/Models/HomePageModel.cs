using AspNetCoreSample.ViewModels;
using System;
using System.Collections.Generic;

namespace AspNetSample.Models
{
    public class HomePageModel
    {
        public int CurrentPage { get; set; } = 1;
        public int Count { get; set; }
        public int PageSize { get; set; } = 10;
        public IEnumerable<OrdersViewModel> Orders { get; set; }
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(Count, PageSize));
    }
}