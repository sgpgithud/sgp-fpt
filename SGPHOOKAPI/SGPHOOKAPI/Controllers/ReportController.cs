using SGPHOOKAPI.Models;
using SGPHOOKAPI.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using SGPHOOKAPI.Utils;

namespace SGPHOOKAPI.Controllers
{
    public class ReportController : ApiController
    {
        SAIGONPOSTDBEntities db = new SAIGONPOSTDBEntities();

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult PhieuGui(string dateFrom, string dateTo, int? status = 0, string mailerid = "")
        {

            if (String.IsNullOrEmpty(dateFrom) || String.IsNullOrEmpty(dateTo))
            {
                dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
                dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                try
                {
                    var dateF = DateTime.ParseExact(dateFrom, "d/M/yyyy", null);
                    var dataT = DateTime.ParseExact(dateTo, "d/M/yyyy", null);

                    dateFrom = dateF.ToString("yyyy-MM-dd");
                    dateTo = dataT.ToString("yyyy-MM-dd");
                }
                catch
                {
                    dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
                    dateTo = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }

            var allMailer = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", "FPTHCM", "KHDN").ToList();

            if (status != -1)
            {
                allMailer = allMailer.Where(p => p.CurrentStatus == status).ToList();
            }


            List<MailerRpt> mailer = new List<MailerRpt>();

            foreach (var item in allMailer)
            {
                mailer.Add(new MailerRpt()
                {
                    MailerID = "*" + item.MailerID + "*",
                    HH = item.OrderType == "HH" ? "X" : "",
                    TL = item.OrderType == "TL" ? "X" : "",
                    OrderContent = item.OrderNote,
                    OrderCode = "*" + item.OrderCode + "*",
                    ReceiverAddress = item.ReceiverContactName + "-" + item.ReceiverName + "\n" + item.ReceiverAddress,
                    SendAddress = item.SenderContactName + "-" + item.SenderName + "\n" + item.SenderAddress,
                    SenderPhone = item.SenderPhone,
                    OrderWeight = item.OrderWeight + " Gram",
                    Price = item.Price != null ? item.Price.Value.ToString("C", Cultures.VietNam) : "0 VND",
                    OrderQuality = item.OrderQuality + "",
                    ReceiverPhone = item.ReceiverPhone,
                    ReceiverDistrict = item.ReceiverProvincePMS,
                    ServicePrice = item.ServicePrice != null ? item.ServicePrice.Value.ToString("C", Cultures.VietNam) : "0 VND",
                    ServiceName = "Chuyển phát nhanh",
                    PaymentName = item.PaymentType == "GN" ? "Thanh toán sau" : "Người gửi thanh toán",
                    PostId = item.PostID,
                    TimeAccept = item.TakeTime + " " + item.TakeDate,
                    COD = item.COD != null ? item.COD.Value.ToString("C", Cultures.VietNam) : "0 VND",
                });
            }

            ReportUtils rUtils = new ReportUtils();
            Stream stream = rUtils.GetReportStream(ReportPath.RptPhieuGui, mailer);

            //  return File(stream, "application/msword", mailerId + ".doc");
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "phieugui.pdf";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            result.Content.Headers.ContentLength = stream.Length;
            var response = ResponseMessage(result);
            return response;

        }


        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult InMotPhieu(string mailerid = "")
        {

            var item = db.MailerInfoes.Where(p => p.MailerID == mailerid).FirstOrDefault();

            List<MailerRpt> mailer = new List<MailerRpt>();

            mailer.Add(new MailerRpt()
            {
                MailerID = "*" + item.MailerID + "*",
                HH = item.OrderType == "HH" ? "X" : "",
                TL = item.OrderType == "TL" ? "X" : "",
                OrderContent = item.OrderNote,
                OrderCode = "*" + item.OrderCode + "*",
                ReceiverAddress = item.ReceiverName + "\n" + item.ReceiverAddress,
                SendAddress = item.SenderName + "\n" + item.SenderAddress,
                SenderPhone = item.SenderPhone,
                OrderWeight = item.OrderWeight + " Gram",
                Price = item.Price != null ? item.Price.Value.ToString("C", Cultures.VietNam) : "0 VND",
                OrderQuality = item.OrderQuality + "",
                ReceiverPhone = item.ReceiverPhone,
                ReceiverDistrict = item.ReceiverProvincePMS,
                ServicePrice = item.ServicePrice != null ? item.ServicePrice.Value.ToString("C", Cultures.VietNam) : "0 VND",
                ServiceName = "Chuyển phát nhanh",
                PaymentName = item.PaymentType == "GN" ? "Thanh toán sau" : "Người gửi thanh toán",
                PostId = item.PostID,
                TimeAccept = item.TakeDate != null ? item.TakeDate.Value.ToString("hh:mm dd/MM/yyyy"):"",
                COD = item.COD != null ? item.COD.Value.ToString("C", Cultures.VietNam) : "0 VND",
            });

            ReportUtils rUtils = new ReportUtils();
            Stream stream = rUtils.GetReportStream(ReportPath.RptPhieuGui, mailer);

            //  return File(stream, "application/msword", mailerId + ".doc");
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "phieugui.pdf";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            result.Content.Headers.ContentLength = stream.Length;
            var response = ResponseMessage(result);
            return response;

        }

    }

}

