using SGPHOOKAPI.Models;
using SGPHOOKAPI.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Cors;
using SGPHOOKAPI.Utils;
using System.Web.Mvc;

namespace SGPHOOKAPI.Controllers
{
    public class ReportController : Controller
    {
        SAIGONPOSTDBEntities db = new SAIGONPOSTDBEntities();

        [HttpGet]
        public ActionResult PhieuGui(string dateFrom, string dateTo, int? status = 0, string mailerid = "", string postId = "KHDN")
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
            var findSendCode = db.CustomerPosts.Where(p => p.PostID == postId && p.GroupID == "FPT").FirstOrDefault();
            var allMailer = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", findSendCode.CustomerID, findSendCode.PostID).ToList();

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



            Stream stream = rUtils.GetReportStream(findPathReportFPT(postId), mailer, ".pdf");

            //  return File(stream, "application/msword", mailerId + ".doc");
            /*
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "phieugui.doc";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(rUtils.GetContentType(".doc"));
            result.Content.Headers.ContentLength = stream.Length;
            var response = ResponseMessage(result);
            return response;
            */

            return File(stream, "application/pdf", "phieuin" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf");

        }


        [HttpGet]
        public ActionResult InMotPhieu(int id)
        {

            var item = db.MailerInfoes.Where(p => p.Id == id).FirstOrDefault();

            List<MailerRpt> mailer = new List<MailerRpt>();

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
                TimeAccept = item.TakeDate != null ? item.TakeDate.Value.ToString("hh:mm dd/MM/yyyy"):"",
                COD = item.COD != null ? item.COD.Value.ToString("C", Cultures.VietNam) : "0 VND",
            });

            ReportUtils rUtils = new ReportUtils();
            Stream stream = rUtils.GetReportStream(findPathReportFPT(item.PostID), mailer, ".pdf");


            return File(stream, "application/msword", "phieuin" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf");

        }

        [HttpGet]
        public ActionResult InMotPhieuPSD(int id)
        {

            var item = db.MailerInfoes.Where(p => p.Id == id).FirstOrDefault();

            List<MailerRpt> mailer = new List<MailerRpt>();

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
                TimeAccept = item.TakeDate != null ? item.TakeDate.Value.ToString("hh:mm dd/MM/yyyy") : "",
                COD = item.COD != null ? item.COD.Value.ToString("C", Cultures.VietNam) : "0 VND",
            });

            ReportUtils rUtils = new ReportUtils();
            Stream stream = rUtils.GetReportStream(ReportPath.RptPhieuGuiPSDCTHO, mailer, ".pdf");

            //  return File(stream, "application/msword", mailerId + ".doc");
            /*
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "phieugui.doc";

            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(rUtils.GetContentType(".doc"));

            result.Content.Headers.ContentLength = stream.Length;
            var response = ResponseMessage(result);
            return response;

    */
            return File(stream, "application/msword", "phieuin" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf");

        }

        private string findPathReportFPT(string postId)
        {
            if (postId == "KHDN")
            {
                return ReportPath.RptPhieuGuiFPTHCM;
            } else if (postId == "CTHO")
            {
                return ReportPath.RptPhieuGuiFPTCTO;
            }
            if (postId == "KV03")
            {
                return ReportPath.RptPhieuGuiFPTDNI;
            }

            return ReportPath.RptPhieuGui;
        }

        [HttpGet]
        public ActionResult PhieuGuiPSD(string dateFrom, string dateTo, int? status = 0, string mailerid = "", string cusid = "")
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
            AspNetUser aspNetUser = db.AspNetUsers.Where(p => p.PostID == "CTHO" && p.CusID == cusid).FirstOrDefault();
            var allMailer = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", aspNetUser.PMSUser, "CTHO").ToList();

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
            Stream stream = rUtils.GetReportStream(ReportPath.RptPhieuGuiPSDCTHO, mailer, ".pdf");

            //  return File(stream, "application/msword", mailerId + ".doc");
            /*
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "phieugui.doc";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(rUtils.GetContentType(".doc"));
            result.Content.Headers.ContentLength = stream.Length;
            var response = ResponseMessage(result);
            return response;
            */

            return File(stream, "application/msword", "phieuin" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf");

        }


    }

}

