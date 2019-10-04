using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGPHOOKAPI.Models
{
    public class ResultInfo
    {
        public int error { get; set; }

        public string message { get; set; }

        public Object data { get; set; }
    }

    public class OrderAddressRequest
    {
        public string address_code { get; set; }
        public string address_name { get; set; }
        public string full_address { get; set; }
        public string province_area_code { get; set; }
        public string district_area_code { get; set; }
        public string ward_area_code { get; set; }
        public string contact_phone { get; set; }
        public string contact_name { get; set; }
        public string customer_name { get; set; }
    }

    public class OrderItemRequest
    {
        public string product_name { get; set; }
        public string package_weight { get; set; }
        public string package_dimension { get; set; }
    }

    public class OrderRequest
    {
        public string token_key { get; set; }
        public string order_number { get; set; }
        public string no_packs { get; set; }
        public string package_weight { get; set; }
        public string cod { get; set; }
        public string service_type { get; set; }
        public string order_note { get; set; }
        public string payment_type { get; set; }
        public OrderAddressRequest sender_address { get; set; }
        public OrderAddressRequest receiver_address { get; set; }
        public List<OrderItemRequest> orderItem { get; set; }
    }

    public class OrderRequireRequest
    {
        public string token_key { get; set; }

        public string code { get; set; }
    }

    public class OrderTrackingResult
    {
        public string order_number { get; set; }

        public int status { get; set; }

        public string note { get; set; }

        public string date_change { get; set; }

        public string current_province_code { get; set; }

        public string current_district_code { get; set; }

        public string status_date { get; set; }
    }

    public class OrderUpdateRequest
    {
        public string ReceiverAddress { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverPhone { get; set; }

        public string ReceiverDistrict { get; set; }

        public string OrderNote { get; set; }

        public double? OrderWeight { get; set; }

        public int? OrderQuality { get; set; }

        public string OrderService { get; set; }

        public string OrderType { get; set; }

        public string ReceiverProvince { get; set; }

        public double? Price { get; set; }

        public double? ServicePrice { get; set; }

        public string OrderCode { get; set; }

        public string MailerID { get; set; }

        public double? OrderWidth { get; set; }

        public double? OrderHeight { get; set; }

        public double? OrderLength { get; set; }

        public string CommitmentDate { get; set; }
    }

    public class FPTCurrentAddress
    {
        public string province_code { get; set; }

        public string district_code { get; set; }

        public string ward_code { get; set; }
    }

    public class FPTOrderUpdate
    {
        public string app_name { get; set; }

        public string password { get; set; }

        public string order_number { get; set; }

        public int state { get; set; }

        public string note { get; set; }

        public string operation { get; set; }

        public string status_date { get; set; }

        public FPTCurrentAddress current_address { get; set; }

        public string no_pack { get; set; }

        public string package_weight { get; set; }

        public string receiver_name { get; set; }

        public string shipman_name { get; set; }

        public string total_cost { get; set; }

        public string cod { get; set; }

        public string dimension { get; set; }

        public string commitment_date { get; set; }
    }

    public class SynDataInfo
    {
        public string Timestamp { get; set; }

        public string Email_Address { get; set; }

        public string Ma_nguoi_gui { get; set; }

        public string UserSend { get; set; }

        public string So_van_don { get; set; }

        public string Dia_chi_phat { get; set; }

        public string So_phieu { get; set; }

        public string tinh_thanh_nhan { get; set; }

        public string quan_huyen_nhan { get; set; }

        public string HTTT { get; set; }

        public string LH { get; set; }

        public string DV { get; set; }

        public string SL { get; set; }

        public string TL { get; set; }

        public string Phu_phi { get; set; }

        public string Status { get; set; }

        public string Du_kien { get; set; }

        public string Ma_tinh { get; set; }

        public string Ma_quan { get; set; }

        public string Quan_pms { get; set; }

        public string Code { get; set; }

        public string Macuoc { get; set; }

        public string Fee { get; set; }
    }
}