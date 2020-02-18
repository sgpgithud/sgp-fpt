using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OfficeOpenXml;
using SGPHOOKAPI.Models;
using SGPHOOKAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;

namespace SGPHOOKAPI.Controllers
{
    public class SGPHubController : ApiController
    {
        SAIGONPOSTDBEntities db = new SAIGONPOSTDBEntities();

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));


        private ApplicationUser AuthLogin(string token)
        {
            try
            {
                token = XString.FromBase64(token);
                var arrtok = token.Split(':');

                if (arrtok.Length != 2)
                    throw new Exception("Wrong authorization format");

                string UserName = arrtok[0];
                string PassWord = arrtok[1];

                var user = UserManager.Find(UserName, PassWord);

                return user;
            }
            catch
            {
                return null;
            }
        }


        [HttpPost]
        public ResultInfo CheckLogin()
        {
            logger.Info("Send check login...");
            ResultInfo result = new ResultInfo()
            {
                error = 0
            };


            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                logger.Info(requestContent);
                var paser = jsonserializer.Deserialize<CheckUserRequest>(requestContent);

                var user = UserManager.Find(paser.user, paser.password);

                if (user != null)
                {
                    if (user.CusID != "SGP")
                        throw new Exception();

                    result.data = XString.ToBase64(paser.user + ":" + paser.password);

                }
                else
                {
                    throw new Exception("");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                result.error = 1;
                result.message = e.Message;
            }

            return result;

        }


        [HttpGet]
        public ResultInfo GetProvince()
        {
            var data = db.Provinces.ToList();


            return new ResultInfo()
            {
                error = 0,
                data = data
            };
        }

        [HttpGet]
        public ResultInfo GetDistrict(string ProvincePMS)
        {

            var findProvince = db.Provinces.Where(p => p.ProvincePMS == ProvincePMS).FirstOrDefault();

            if (findProvince == null)
            {
                return new ResultInfo()
                {
                    error = 0,
                    data = new List<District>()
                };
            }

            var data = db.Districts.Where(p => p.ProvinceId == findProvince.Id).ToList();

            return new ResultInfo()
            {
                error = 0,
                data = data
            };
        }

        [HttpPost]
        public ResultInfo SendOrderAsync()
        {
            logger.Info("Send order...");
            ResultInfo result = new ResultInfo()
            {
                error = 0
            };

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                logger.Info(requestContent);
                var paser = jsonserializer.Deserialize<OrderRequest>(requestContent);

                var findUser = AuthLogin(paser.token_key);

                if (findUser == null)
                    throw new Exception("User sai");

                var findOrder = db.MailerInfoes.Where(p => p.OrderCode == paser.order_number && p.SendId == findUser.PMSUser).FirstOrDefault();

                string str1 = "Đã tạo đơn hàng " + findUser.PMSUser + "  : " + paser.order_number;

                if (findOrder != null)
                {
                    int? currentStatus = findOrder.CurrentStatus;

                    if (currentStatus == 3)
                        throw new Exception("Đơn hàng đã in bill, không thể chỉnh sửa");


                    // xoa don hang
                    var findItems = db.OrderItems.Where(p => p.OrderId == findOrder.Id).ToList();

                    db.OrderItems.RemoveRange(findItems);

                    var findTrackings = db.TrackingOrders.Where(p => p.OrderId == findOrder.Id).ToList();
                    db.TrackingOrders.RemoveRange(findTrackings);

                    db.MailerInfoes.Remove(findOrder);

                    db.SaveChanges();
                }

                var order = new MailerInfo()
                {
                    CreateDate = DateTime.Now,
                    CurrentStatus = 0,
                    CustomerId = findUser.PMSUser,
                    MailerID = "",
                    OrderCode = paser.order_number,
                    OrderNote = paser.order_note,
                    OrderService = paser.service_type,
                    OrderType = "HH",
                    ReceiverName = paser.receiver_address.customer_name,
                    ReceiverAddress = paser.receiver_address.full_address,
                    ReceiverDistrict = paser.receiver_address.district_area_code,
                    ReceiverPhone = paser.receiver_address.contact_phone,
                    ReceiverProvince = paser.receiver_address.province_area_code,
                    StatusNotes = "Mới nhận",
                    OrderHeight = 0.0,
                    OrderWeight = Convert.ToDouble(paser.package_weight) * 1000,
                    OrderLength = 0.0,
                    OrderQuality = Convert.ToDouble(paser.no_packs),
                    OrderWidth = 0,
                    Price = 0,
                    COD = Convert.ToDouble(paser.cod),
                    ServicePrice = 0,
                    ModifyDate = new DateTime?(DateTime.Now),
                    UserSend = findUser.Email,
                    PostID = findUser.PostID,
                    CurrentPost = findUser.PostID,
                    SendId = findUser.PMSUser,
                    SenderAddress = paser.sender_address.full_address,
                    SenderName = paser.sender_address.address_name,
                    SenderPhone = paser.sender_address.contact_phone,
                    SenderProvinceID = paser.sender_address.province_area_code,
                    SenderDistrictID = paser.sender_address.ward_area_code,
                    SendToWavehoure = 0,
                    SGPStatus = 0,
                    SenderContactName = paser.sender_address.contact_name,
                    ReceiverContactName = paser.receiver_address.contact_name
                };

                db.MailerInfoes.Add(order);
                db.SaveChanges();

                result.message = str1;

                PostOffice postOffice = db.PostOffices.Where(p => p.PostOfficeID == findUser.PostID).FirstOrDefault();

                this.db.TrackingOrders.Add(new TrackingOrder()
                {
                    CreateTime = new DateTime?(DateTime.Now),
                    OrderId = order.Id,
                    Status = order.CurrentStatus,
                    StatusName = "Đã tạo đơn hàng",
                    CurrentPost = postOffice.PostOfficeID,
                    DistrictId = postOffice.DistrictID,
                    ProvinceId = postOffice.ProvinceID
                });
                this.db.SaveChanges();

                string str2 = "";
                foreach (OrderItemRequest orderItenInfo in paser.orderItem)
                {
                    this.db.OrderItems.Add(new OrderItem()
                    {
                        PackageDimension = orderItenInfo.package_dimension,
                        PackagetName = orderItenInfo.product_name,
                        PackageWeight = Convert.ToDouble(orderItenInfo.package_weight),
                        OrderId = order.Id
                    });
                    this.db.SaveChanges();
                    str2 = str2 + orderItenInfo.product_name + "(TL: " + (object)orderItenInfo.package_weight + ", KT: " + orderItenInfo.package_dimension + " ),";
                }
                order.OrderContent = str2;
                db.Entry(order).State = System.Data.Entity.EntityState.Modified;

                this.db.SaveChanges();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                result.error = 1;
                result.message = e.Message;
            }

            return result;
        }



        [HttpGet]
        [Authorize]
        [EnableCors("*", "*", "*")]
        public ResultInfo GetOrdeByStatus(string mailerid, string cusid, string dateFrom, string dateTo, int? status = 0, int? deliveryStatus = 0)
        {
            logger.Info("Send GetOrderFPTByStatus...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                message = "Đã thêm",
                data = (object)new List<MailerInfo>()
            };

            try
            {
                DateTime exact1 = DateTime.ParseExact(dateFrom, "d/M/yyyy", (IFormatProvider)null);
                DateTime exact2 = DateTime.ParseExact(dateTo, "d/M/yyyy", (IFormatProvider)null);
                dateFrom = exact1.ToString("yyyy-MM-dd");
                dateTo = exact2.ToString("yyyy-MM-dd");
            }
            catch
            {
                dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
                dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            }

            var findUser = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).FirstOrDefault();

            var findCus = db.AspNetUsers.Where(p => p.PostID == findUser.PostID && p.CusID == cusid).FirstOrDefault();

            if (findCus == null)
                return resultInfo;

            List<get_mailers_Result> findMailers = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", findCus.PMSUser, findCus.PostID).Where(p => p.CurrentStatus == status).ToList();

            if (status == 3)
            {
                // Đang nhân
                if (deliveryStatus != 0)
                {
                    findMailers = findMailers.Where(p => p.SGPStatus == deliveryStatus).ToList();
                }
            }

            resultInfo.data = findMailers;
            return resultInfo;
        }


        [HttpPost]
        [Authorize]
        public ResultInfo UpdateOrder()
        {
            logger.Info("Send GetOrderFPTByStatus...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                message = "Đã thêm",
                data = (object)new List<MailerInfo>()
            };

            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                logger.Info(requestContent);
                var paser = jsonserializer.Deserialize<UpdateRequestOrder>(requestContent);

                var findMailer = db.MailerInfoes.Where(p => p.Id == paser.id).FirstOrDefault();

                findMailer.Price = paser.price;
                findMailer.ServicePrice = paser.priceService;
                findMailer.CurrentStatus = 3;
                findMailer.ReceiverProvincePMS = paser.provincePMS;
                findMailer.ReceiverDistrictPMS = paser.districtPMS;
                findMailer.OrderWeight = paser.weight;
                findMailer.OrderType = paser.orderType;
                findMailer.OrderService = paser.orderService;
                findMailer.OrderQuality = paser.quantity;
                findMailer.MailerID = paser.mailerid;
                findMailer.OrderNote = paser.notes;

                db.Entry(findMailer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                db.TrackingOrders.Add(new TrackingOrder()
                {
                    CreateTime = new DateTime?(DateTime.Now),
                    OrderId = findMailer.Id,
                    Status = 3,
                    StatusName = "Đã nhận hàng"
                });
                this.db.SaveChanges();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                resultInfo.error = 1;
                resultInfo.message = e.Message;
            }

            return resultInfo;

        }


        [HttpGet]
        [EnableCors("*", "*", "*")]
        public IHttpActionResult GetOrderExcel(string dateFrom, string dateTo, int? status = 0, int? deliveryStatus = 0, string mailerid = "", string cusid = "")
        {

            //  AspNetUser findUser = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).FirstOrDefault();

            DateTime now = DateTime.Now;
            try
            {
                DateTime exact1 = DateTime.ParseExact(dateFrom, "d/M/yyyy", (IFormatProvider)null);
                DateTime exact2 = DateTime.ParseExact(dateTo, "d/M/yyyy", (IFormatProvider)null);
                dateFrom = exact1.ToString("yyyy-MM-dd");
                dateTo = exact2.ToString("yyyy-MM-dd");
            }
            catch
            {
                dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
                now = DateTime.Now;
                dateTo = now.ToString("yyyy-MM-dd");
            }


            AspNetUser aspNetUser = db.AspNetUsers.Where(p => p.PostID == "CTHO" && p.CusID == cusid).FirstOrDefault();
            List<get_mailers_Result> allCG = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", aspNetUser.PMSUser, "CTHO").ToList<get_mailers_Result>();

            allCG = allCG.Where(p => p.CurrentStatus == status).ToList();

            if (status == 3)
            {
                // Đang nhân
                if (deliveryStatus != 0)
                {
                    allCG = allCG.Where(p => p.SGPStatus == deliveryStatus).ToList();
                }
            }


            string sourceFileName = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/phieugui.xlsx");
            now = DateTime.Now;
            string str = System.Web.Hosting.HostingEnvironment.MapPath("~/Temp/" + ("report" + now.ToString("ddMMyyyyHHmmss") + ".xlsx"));
            System.IO.File.Copy(sourceFileName, str);
            FileInfo fileInfo = new FileInfo(str);
            ExcelPackage package = null;
            try
            {
                using (package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["CCG11"];

                    for (var index = 0; index < allCG.Count(); index++)
                    {
                        double? Price = allCG[index].Price == null ? 0 : allCG[index].Price;
                        double? PriceCOD = allCG[index].PriceCOD == null ? 0 : allCG[index].PriceCOD;
                        double? ServicePrice = allCG[index].ServicePrice == null ? 0 : allCG[index].ServicePrice;



                        worksheet.Cells[index + 2, 1].Value = allCG[index].CreateDate;
                        worksheet.Cells[index + 2, 2].Value = allCG[index].CreateTime;
                        worksheet.Cells[index + 2, 3].Value = allCG[index].MailerID;

                        worksheet.Cells[index + 2, 4].Value = allCG[index].PaymentType;
                        worksheet.Cells[index + 2, 5].Value = allCG[index].PostID;

                        worksheet.Cells[index + 2, 6].Value = allCG[index].SendId;
                        worksheet.Cells[index + 2, 7].Value = allCG[index].SenderName;

                        worksheet.Cells[index + 2, 8].Value = allCG[index].OrderType;
                        worksheet.Cells[index + 2, 9].Value = allCG[index].OrderService;
                        worksheet.Cells[index + 2, 10].Value = allCG[index].OrderQuality;

                        worksheet.Cells[index + 2, 11].Value = allCG[index].OrderWeight;
                        worksheet.Cells[index + 2, 12].Value = allCG[index].OrderWeight;

                        worksheet.Cells[index + 2, 13].Value = allCG[index].ReceiverAddress;
                        worksheet.Cells[index + 2, 14].Value = allCG[index].ReceiverProvincePMS;
                        worksheet.Cells[index + 2, 15].Value = allCG[index].ReceiverDistrictPMS;

                        worksheet.Cells[index + 2, 16].Value = Price;
                        worksheet.Cells[index + 2, 17].Value = ServicePrice;
                        worksheet.Cells[index + 2, 19].Value = Price + ServicePrice;

                        worksheet.Cells[index + 2, 23].Value = 0;

                        worksheet.Cells[index + 2, 24].Value = allCG[index].SenderAddress;
                        // worksheet.Cells[index + 2, 25].Value = allCG[index].SenderPhone;
                        worksheet.Cells[index + 2, 29].Value = allCG[index].ReceiverName;
                        //  worksheet.Cells[index + 2, 30].Value = allCG[index].ReceiverPhone;
                        worksheet.Cells[index + 2, 33].Value = allCG[index].OrderNote;
                        worksheet.Cells[index + 2, 35].Value = allCG[index].PostID;
                        worksheet.Cells[index + 2, 36].Value = allCG[index].COD;
                        worksheet.Cells[index + 2, 37].Value = allCG[index].OrderNote;

                    }
                    package.Save();
                    package.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (package != null)
                {
                    package.Dispose();
                }
            }

            FileStream stream = fileInfo.OpenRead();

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "phieugui" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xls";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
            result.Content.Headers.ContentLength = stream.Length;
            var response = ResponseMessage(result);
            return response;
        }


        [HttpGet]
        public ResultInfo callPrice(int quantity, float weight, string postOfficeId, string proviceId, string customerid, string serviceType)
        {
            var result = new ResultInfo()
            {
                error = 0
            };

            PMSSGPDBEntities pMSSGPDB = new PMSSGPDBEntities();

            // tim khoảng cách
            var findDistance = pMSSGPDB.MM_Distances.Where(p => p.PostOfficeID == postOfficeId && p.ProvinceID == proviceId).FirstOrDefault();

            if (findDistance == null)
            {
                result.error = 1;
                return result;
            }

            // tìm mã bảng giá
            var findPriceMatrix = pMSSGPDB.MM_CallPrice_getMatrix_test(DateTime.Now, postOfficeId, serviceType, customerid, proviceId, (int)findDistance.Distance, "1", "N", "KV02").FirstOrDefault();

            if (findPriceMatrix == null)
            {
                result.error = 1;
                return result;
            }

            // tim khoang cach
            var findAllPice = pMSSGPDB.MM_CallPrice_Test(findPriceMatrix.PriceMatrixID, DateTime.Now, postOfficeId, serviceType, customerid, proviceId, (int)findDistance.Distance, "1", "N", "KV02").ToList();

            // tinh gia
            double? price = 0;

            if (weight <= 2000)
            {
                var findPrice = findAllPice.Where(p => p.RangeWeightFrom <= weight && p.RangeWeightTo >= weight && p.IsNext == false).FirstOrDefault();

                price = findPrice.Price;


                result.data = price;


            } else
            {
                var findPrice = findAllPice.Where(p => p.RangeWeightTo == 2000 && p.IsNext == false).FirstOrDefault();

                price = findPrice.Price;

                double? weightTem = weight - 2000;

                var findPriceNext = findAllPice.Where(p => p.IsNext == true).FirstOrDefault();

                double? a = weightTem / findPriceNext.RangeWeightTo;

                price = price + (Math.Round((double)a) * findPriceNext.Price);

                result.data = price;
            }


            return result;
        }

        [HttpGet]
        [EnableCors("*", "*", "*")]
        public ResultInfo GetUserInfo()
        {
            var result = new ResultInfo()
            {
                error = 0
            };

            var findUser = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).FirstOrDefault();

            result.data = new
            {
                PostID = findUser.PostID,
                UserName = findUser.UserName
            };

            return result;
        }

    }
}