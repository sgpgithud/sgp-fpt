using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGPHOOKAPI.Models
{
    public class CheckUserRequest
    {
        public string user { get; set; }

        public string password { get; set; }
    }

    public class UpdateRequestOrder
    {
        public string mailerid { get; set; }

        public int id { get; set; }

        public double price { get; set; }

        public double priceService { get; set; }

        public string provincePMS { get; set; }

        public string districtPMS { get; set; }

        public string orderType { get; set; }

        public string orderService { get; set; }

        public int quantity { get; set; }

        public int weight { get; set; }

        public string address { get; set; }

        public string notes { get; set; }



    }

    public class FindOrderRequest
    {
        public string token { get; set; }
        public string mailerid { get; set; }

        public string dateFrom { get; set; }

        public string dateTo { get; set; }

        public int status { get; set; }

        public string cusId { get; set; }

    }
}