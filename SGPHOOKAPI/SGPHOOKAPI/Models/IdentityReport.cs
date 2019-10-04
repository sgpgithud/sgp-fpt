using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGPHOOKAPI.Models
{

    public class MailerRpt
    {
        public string MailerID { get; set; }
        public string SendAddress { get; set; }
        public string ReceiverAddress { get; set; }
        public string SenderPhone { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverDistrict { get; set; }
        public string OrderContent { get; set; }
        public string OrderNote { get; set; }
        public string OrderWeight { get; set; }
        public string OrderQuality { get; set; }
        public string OrderService { get; set; }
        public string OrderType { get; set; }
        public string OrderCode { get; set; }
        public string Price { get; set; }
        public string ServicePrice { get; set; }
        public string PriceTotal { get; set; }
        public string COD { get; set; }


        public string HH { get; set; }

        public string TL { get; set; }

        public string PostId { get; set; }

        public string ServiceName { get; set; }

        public string PaymentName { get; set; }

        public string TimeAccept { get; set; }


    }
}