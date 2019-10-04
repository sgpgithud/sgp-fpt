using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OfficeOpenXml;
using SGPHOOKAPI.Models;
using System.Net.Http.Headers;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SGPHOOKAPI.Utils;
using System.Text;
using System.IO;
using SGPHOOKAPI.SGPService;

namespace SGPHOOKAPI.Controllers
{
    public class FPTHookController : ApiController
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
        public ResultInfo FPTOrderAsync()
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

                var findOrder = db.MailerInfoes.Where(p => p.OrderCode == paser.order_number).FirstOrDefault();
                string str1 = "Đã tạo đơn hàng " + paser.order_number;

                if (findOrder != null)
                {
                    int? currentStatus = findOrder.CurrentStatus;
                    int num = 0;
                    if ((currentStatus.GetValueOrDefault() == num ? (!currentStatus.HasValue ? 1 : 0) : 1) != 0)
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


        [HttpPost]
        public ResultInfo CancelOrderAsync()
        {
            ///   FPTHookController.logger.Info("cancel order...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0
            };
            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                //   logger.Info(requestContent);

                var paser = jsonserializer.Deserialize<OrderRequireRequest>(requestContent);

                if (this.AuthLogin(paser.token_key) == null)
                    throw new Exception("Wrong authorication !!!");

                MailerInfo find = db.MailerInfoes.Where(p => p.OrderCode == paser.code).FirstOrDefault();

                if (find == null)
                    throw new Exception("Không tìm thấy thông tin");

                if (find.CurrentStatus != 0)
                    throw new Exception("Đơn hàng đã xác nhận, không thể hủy");
                PostOffice postOffice = db.PostOffices.Where(p => p.PostOfficeID == find.CurrentPost).FirstOrDefault();

                find.CurrentStatus = 1;
                find.StatusNotes = "Đã hủy đơn hàng";

                find.ModifyDate = new DateTime?(DateTime.Now);
                db.Entry(find).State = System.Data.Entity.EntityState.Modified;
                this.db.SaveChanges();
                this.db.TrackingOrders.Add(new TrackingOrder()
                {
                    CreateTime = new DateTime?(DateTime.Now),
                    OrderId = new long?((long)find.Id),
                    Status = find.CurrentStatus,
                    StatusName = "Đã hủy đơn hàng",
                    CurrentPost = find.CurrentPost,
                    ProvinceId = postOffice != null ? postOffice.ProvinceID : "",
                    DistrictId = postOffice != null ? postOffice.DistrictID : ""
                });

                this.db.SaveChanges();
                resultInfo.message = "Đã hủy đơn hàng " + find.OrderCode;
            }
            catch (Exception ex)
            {
                //  FPTHookController.logger.Error(ex.Message);
                resultInfo.error = 1;
                resultInfo.message = ex.Message;
            }

            return resultInfo;
        }

        [HttpPost]
        public ResultInfo GetOrderFPTInfoAsync()
        {
            //  logger.Info("Send order...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                message = "Đã thêm",
                data = (object)new List<MailerInfo>()
            };
            try
            {
                string result = Request.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
                //   FPTHookController.logger.Info(result);
                OrderRequireRequest paser = scriptSerializer.Deserialize<OrderRequireRequest>(result);
                if (this.AuthLogin(paser.token_key) == null)

                    throw new Exception("Wrong authorication !!!");
                List<get_mailer_fpt_Result> listOrders = db.get_mailer_fpt("%" + paser.code + "%").ToList();

                resultInfo.data = listOrders;
            }
            catch (Exception ex)
            {
                //      logger.Error(ex.Message);
                resultInfo.error = 1;
                resultInfo.message = ex.Message;
            }
            return resultInfo;
        }

        [HttpPost]
        public ResultInfo TrackingOrderFPTAsync()
        {
            //   FPTHookController.logger.Info("Send order...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                message = "Đã thêm",
                data = (object)new List<MailerInfo>()
            };
            try
            {
                string result = Request.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
                //      FPTHookController.logger.Info(result);
                var paser = scriptSerializer.Deserialize<OrderRequireRequest>(result);

                if (this.AuthLogin(paser.token_key) == null)
                    throw new Exception("Wrong authorication !!!");
                get_mailer_fpt_Result find = db.get_mailer_fpt(paser.code).FirstOrDefault();

                if (find == null)
                    throw new Exception("Không tìm thấy đơn hàng");

                var orderedEnumerable = db.TrackingOrders.Where(p => p.OrderId == find.Id).OrderBy(p => p.CreateTime).ToList();

                List<OrderTrackingResult> orderTrackingResultList = new List<OrderTrackingResult>();
                foreach (TrackingOrder trackingOrder in (IEnumerable<TrackingOrder>)orderedEnumerable)
                    orderTrackingResultList.Add(new OrderTrackingResult()
                    {
                        date_change = trackingOrder.CreateTime.Value.ToString("dd/MM/yyyy HH:mm"),
                        note = trackingOrder.StatusName,
                        order_number = find.OrderCode,
                        status = trackingOrder.Status.Value,
                        current_district_code = trackingOrder.ProvinceId,
                        current_province_code = trackingOrder.DistrictId,
                        status_date = trackingOrder.CreateTime.Value.ToString("dd/MM/yyyy HH:mm")
                    });
                resultInfo.data = orderTrackingResultList;
            }
            catch (Exception ex)
            {

                logger.Error(ex.Message);
                resultInfo.error = 1;
                resultInfo.message = ex.Message;
            }
            return resultInfo;
        }

        [HttpGet]
        [Authorize]
        [EnableCors("*", "*", "*")]
        public ResultInfo GetOrderFPTByStatus(string mailerid, string dateFrom, string dateTo, int? status = 0, int? deliveryStatus = 0)
        {
            //    logger.Info("Send GetOrderFPTByStatus...");
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

            var findCus = db.AspNetUsers.Where(p => p.PostID == findUser.PostID && p.CusID == "FPT").FirstOrDefault();

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
        [EnableCors("*", "*", "*")]
        [Authorize]
        public ResultInfo SGPUpdateOrder()
        {
            //   FPTHookController.logger.Info("Send update order...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0
            };
            try
            {
                string result = Request.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
                //    logger.Info(result);
                var paser = scriptSerializer.Deserialize<OrderUpdateRequest>(result);

                MailerInfo mailerInfo = db.MailerInfoes.Where(p => p.OrderCode == paser.OrderCode).FirstOrDefault();

                if (mailerInfo == null)
                    throw new Exception("Sai mã");
                int? currentStatus = mailerInfo.CurrentStatus;
                int num = 0;
                if ((currentStatus.GetValueOrDefault() == num ? (!currentStatus.HasValue ? 1 : 0) : 1) != 0)
                    throw new Exception("Không thể update");
                mailerInfo.ReceiverProvincePMS = paser.ReceiverProvince;
                mailerInfo.ReceiverDistrictPMS = paser.ReceiverDistrict;
                mailerInfo.MailerID = paser.MailerID;
                mailerInfo.Price = paser.Price;
                mailerInfo.ServicePrice = paser.ServicePrice;
                mailerInfo.UserTake = User.Identity.Name;
                mailerInfo.OrderNote = paser.OrderNote;
                mailerInfo.OrderService = paser.OrderService;
                mailerInfo.OrderQuality = paser.OrderQuality;
                mailerInfo.OrderType = paser.OrderType;
                mailerInfo.OrderWeight = paser.OrderWeight;
                mailerInfo.OrderHeight = paser.OrderHeight;
                mailerInfo.OrderLength = paser.OrderLength;
                mailerInfo.OrderWidth = paser.OrderWidth;
                mailerInfo.CommitmentDate = paser.CommitmentDate;
                if (!string.IsNullOrEmpty(mailerInfo.MailerID))
                    mailerInfo.CurrentStatus = 2;

                db.Entry(mailerInfo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                FPTOrderUpdate data = new FPTOrderUpdate();
                data.state = 2;
                data.operation = "Confirmed";
                data.no_pack = string.Concat((object)mailerInfo.OrderQuality);
                data.note = "Confirmed pick up";
                data.order_number = mailerInfo.OrderCode;
                data.status_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.package_weight = string.Concat((object)mailerInfo.OrderWeight);
                data.receiver_name = mailerInfo.ReceiverName;
                data.cod = string.Concat((object)mailerInfo.COD);
                FPTOrderUpdate fptOrderUpdate = data;
                double? price = mailerInfo.Price;
                double? nullable1 = mailerInfo.PriceCOD;
                double? nullable2 = price.HasValue & nullable1.HasValue ? new double?(price.GetValueOrDefault() + nullable1.GetValueOrDefault()) : new double?();
                double? servicePrice = mailerInfo.ServicePrice;
                double? nullable3;
                if (!(nullable2.HasValue & servicePrice.HasValue))
                {
                    nullable1 = new double?();
                    nullable3 = nullable1;
                }
                else
                    nullable3 = new double?(nullable2.GetValueOrDefault() + servicePrice.GetValueOrDefault());
                string str = string.Concat((object)nullable3);
                fptOrderUpdate.total_cost = str;
                data.dimension = mailerInfo.OrderWidth.ToString() + "x" + (object)mailerInfo.OrderHeight + "x" + (object)mailerInfo.OrderLength;
                data.shipman_name = "";
                data.commitment_date = mailerInfo.CommitmentDate;
                if (SGPUtils.SendUpdateFPT(data) == null)
                    throw new Exception("Error send update FPT");


            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                resultInfo.error = 1;
                resultInfo.message = ex.Message;
            }
            return resultInfo;
        }

        [HttpGet]
        [EnableCors("*", "*", "*")]
        public ResultInfo SynData()
        {
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0
            };
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sheetdb.io/api/v1/705c6uyey9vod");
            httpWebRequest.Method = "GET";
            UTF8Encoding utF8Encoding = new UTF8Encoding();
            httpWebRequest.ContentType = "application/json";
            long num1 = 0;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    num1 = response.ContentLength;
                    List<SynDataInfo> source = new JavaScriptSerializer().Deserialize<List<SynDataInfo>>(new StreamReader(response.GetResponseStream(), (Encoding)utF8Encoding).ReadToEnd());
                    var mailerInfoes = db.MailerInfoes.Where(p => p.CurrentStatus == 2).ToList();

                    foreach (MailerInfo item in mailerInfoes)
                    {

                        SynDataInfo synDataInfo = source.Where<SynDataInfo>((Func<SynDataInfo, bool>)(p => p.So_van_don == item.OrderCode)).FirstOrDefault<SynDataInfo>();
                        if (synDataInfo != null)
                        {
                            string str1 = "";
                            try
                            {
                                str1 = DateTime.ParseExact(synDataInfo.Du_kien, "d/M/yyyy HH:mm:ss", (IFormatProvider)null).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            catch
                            {
                            }
                            item.MailerID = synDataInfo.So_van_don;
                            item.Price = Convert.ToDouble(string.IsNullOrEmpty(synDataInfo.Fee) ? "0" : synDataInfo.Fee);
                            item.OrderService = synDataInfo.DV;
                            item.PaymentType = synDataInfo.HTTT;
                            item.OrderType = synDataInfo.LH;
                            item.OrderQuality = string.IsNullOrEmpty(synDataInfo.SL) ? 1 : Convert.ToInt32(synDataInfo.SL);
                            item.OrderWeight = new double?(Convert.ToDouble(synDataInfo.TL));
                            item.CommitmentDate = str1;
                            item.ReceiverAddress = synDataInfo.Dia_chi_phat;
                            item.ReceiverProvincePMS = synDataInfo.Code;
                            item.ReceiverDistrictPMS = synDataInfo.Quan_pms;
                            item.ServicePrice = Convert.ToDouble(string.IsNullOrEmpty(synDataInfo.Phu_phi) ? "0" : synDataInfo.Fee);
                            item.CurrentStatus = 3;
                            item.StatusNotes = "Đã nhận hàng";
                            item.TakeDate = DateTime.Now;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            db.TrackingOrders.Add(new TrackingOrder()
                            {
                                CreateTime = new DateTime?(DateTime.Now),
                                OrderId = item.Id,
                                Status = 3,
                                StatusName = "Đã nhận hàng"
                            });
                            this.db.SaveChanges();
                            FPTOrderUpdate data = new FPTOrderUpdate();
                            data.state = 2;
                            data.operation = "Confirmed";
                            data.no_pack = string.Concat((object)item.OrderQuality);
                            data.note = "Confirmed pick up";
                            data.order_number = item.OrderCode;
                            data.status_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            FPTOrderUpdate fptOrderUpdate = data;
                            double? orderWeight = item.OrderWeight;
                            double num2 = 1000.0;
                            string str2 = orderWeight != null ? (Convert.ToDouble(orderWeight.GetValueOrDefault()) / num2) + "" : "0";
                            fptOrderUpdate.package_weight = str2;
                            data.receiver_name = item.ReceiverName;
                            data.cod = item.COD + "";
                            data.total_cost = (item.Price + item.ServicePrice) * 1000 + "";
                            data.dimension = item.OrderWidth.ToString() + "x" + (object)item.OrderHeight + "x" + (object)item.OrderLength;
                            data.shipman_name = "";
                            data.commitment_date = item.CommitmentDate;

                            // log
                            var resultFPT = SGPUtils.SendUpdateFPT(data);
                            logger.Info("send update for FPT: " + new JavaScriptSerializer().Serialize(data) + " |||| " + "result : " + resultFPT);

                        }
                    }
                    return resultInfo;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                resultInfo.error = 1;
                return resultInfo;
            }
        }

        [HttpGet]
        public ResultInfo SendFPTUpdate() 
        {
            var allMailers = db.MailerInfoes.Where(p => p.CurrentStatus == 3).ToList();

            foreach(var item in allMailers)
            {
                FPTOrderUpdate data = new FPTOrderUpdate();
                data.state = 2;
                data.operation = "Confirmed";
                data.no_pack = string.Concat((object)item.OrderQuality);
                data.note = "Confirmed pick up";
                data.order_number = item.OrderCode;
                data.status_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                FPTOrderUpdate fptOrderUpdate = data;
                double? orderWeight = item.OrderWeight;
                double num2 = 1000.0;
                string str2 = orderWeight != null ? (Convert.ToDouble(orderWeight.GetValueOrDefault()) / num2) + "" : "0";
                fptOrderUpdate.package_weight = str2;
                data.receiver_name = item.ReceiverName;
                data.cod = item.COD + "";
                data.total_cost = (item.Price + item.ServicePrice) * 1000 + "";
                data.dimension = item.OrderWidth.ToString() + "x" + (object)item.OrderHeight + "x" + (object)item.OrderLength;
                data.shipman_name = "";
                data.commitment_date = item.CommitmentDate;

                // log
                var resultFPT = SGPUtils.SendUpdateFPT(data);

            }

            return new ResultInfo()
            {
                error = 0,
                message = "success"
            };

        }


        [HttpGet]
        [EnableCors("*", "*", "*")]
        public ResultInfo AcceptOrder(int isAccept, int orderId)
        {
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0
            };
            MailerInfo findOrder = db.MailerInfoes.Where(p => p.Id == orderId).FirstOrDefault();

            if (findOrder != null)
            {
                int? currentStatus = findOrder.CurrentStatus;
                int num1 = 0;
                if ((currentStatus.GetValueOrDefault() == num1 ? (!currentStatus.HasValue ? 1 : 0) : 1) == 0)
                {
                    int num2 = -1;
                    switch (isAccept)
                    {
                        case 0:
                            num2 = 1;
                            break;
                        case 1:
                            num2 = 2;
                            break;
                    }
                    if (num2 == -1)
                    {
                        resultInfo.error = 1;
                        return resultInfo;
                    }
                    findOrder.CurrentStatus = new int?(num2);
                    findOrder.StatusNotes = num2 == 2 ? "Đã nhận đơn hàng" : "Đã hủy đơn hàng";
                    findOrder.ModifyDate = new DateTime?(DateTime.Now);
                    db.Entry(findOrder).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                    PostOffice postOffice = db.PostOffices.Where(p => p.PostOfficeID == findOrder.CurrentPost).FirstOrDefault();

                    this.db.TrackingOrders.Add(new TrackingOrder()
                    {
                        CreateTime = new DateTime?(DateTime.Now),
                        OrderId = new long?((long)findOrder.Id),
                        Status = findOrder.CurrentStatus,
                        StatusName = num2 == 2 ? "Đã nhận đơn hàng" : "Đã hủy đơn hàng",
                        CurrentPost = postOffice.PostOfficeID,
                        DistrictId = postOffice.DistrictID,
                        ProvinceId = postOffice.ProvinceID
                    });

                    this.db.SaveChanges();
                    if (num2 == 2)
                        resultInfo.data = (object)findOrder;
                    return resultInfo;
                }
            }
            resultInfo.error = 1;
            return resultInfo;
        }

        [HttpGet]
        [EnableCors("*", "*", "*")]
        public ResultInfo SynPMS()
        {
            //   FPTHookController.logger.Info("Send SynPMS...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                message = "Đã thêm"
            };

            //     AspNetUser findUser = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).FirstOrDefault();

            //      AspNetUser findCUS = db.AspNetUsers.Where(p => p.PostID == findUser.PostID && p.CusID == "FPT").FirstOrDefault();

            List<MailerInfo> list = db.MailerInfoes.Where(p => p.CurrentStatus == (int?)3 && p.SGPStatus == (int?)0 && p.CustomerId == "FPTHCM").ToList();

            SGPServiceClient sgpServiceClient = new SGPServiceClient();
            try
            {
                foreach (MailerInfo mailerInfo1 in list)
                {
                    MailerInfo item = mailerInfo1;
                    Trackings trackings = ((IEnumerable<Trackings>)sgpServiceClient.ToolTracking(item.MailerID)).FirstOrDefault<Trackings>();
                    if (trackings != null)
                    {
                        item.EmployeeId = trackings.BS_Employees_EmployeeID;
                        item.EmployeeName = trackings.BS_Employees_EmployeeName;
                        item.CurrentPost = trackings.MM_Mailers_CurrentPostOfficeID;
                        item.SGPStatus = new int?(Convert.ToInt32(trackings.MM_Status_StatusID));
                        PostOffice postOffice = db.PostOffices.Where(p => p.PostOfficeID == item.CurrentPost).FirstOrDefault();
                        if (trackings.MM_Status_StatusID == "6")
                        {
                            string detailDeliveryTo = trackings.MM_MailerDeliveryDetail_DeliveryTo;
                            item.DeliveryTo = detailDeliveryTo;
                            item.SGPStatus = new int?(6);
                            item.SGPStatusName = "Đã phát thành công";
                            item.StatusNotes = "Đã phát thành công";
                            item.DeliveryNote = trackings.MM_MailerDeliveryDetail_DeliveryNotes;
                            MailerInfo mailerInfo2 = item;
                            DateTime? detailDeliveryDate = trackings.MM_MailerDeliveryDetail_DeliveryDate;
                            DateTime now;
                            string str1;
                            if (!detailDeliveryDate.HasValue)
                            {
                                str1 = "";
                            }
                            else
                            {
                                detailDeliveryDate = trackings.MM_MailerDeliveryDetail_DeliveryDate;
                                now = detailDeliveryDate.Value;
                                str1 = now.ToShortDateString();
                            }
                            mailerInfo2.DeliveryDate = str1;
                            this.db.TrackingOrders.Add(new TrackingOrder()
                            {
                                CreateTime = new DateTime?(DateTime.Now),
                                OrderId = new long?((long)item.Id),
                                Status = new int?(4),
                                StatusName = "Đã phát thành công",
                                CurrentPost = postOffice.PostOfficeID,
                                DistrictId = postOffice.DistrictID,
                                ProvinceId = postOffice.ProvinceID
                            });
                            this.db.SaveChanges();
                            FPTOrderUpdate data = new FPTOrderUpdate();
                            data.state = 4;
                            data.operation = "Success";
                            data.no_pack = string.Concat((object)item.OrderQuality);
                            data.note = "Đã phát thành công";
                            data.order_number = item.OrderCode;
                            FPTOrderUpdate fptOrderUpdate1 = data;
                            now = DateTime.Now;
                            string str2 = now.ToString("yyyy-MM-dd HH:mm:ss");
                            fptOrderUpdate1.status_date = str2;
                            FPTOrderUpdate fptOrderUpdate2 = data;
                            double? orderWeight = item.OrderWeight;
                            double num = 1000.0;
                            string str3 = string.Concat((object)(orderWeight.HasValue ? new double?(orderWeight.GetValueOrDefault() / num) : new double?()));
                            fptOrderUpdate2.package_weight = str3;
                            data.receiver_name = item.ReceiverName;
                            data.cod = string.Concat((object)item.COD);
                            data.total_cost = (item.Price + item.ServicePrice) * 1000 + "";
                            data.dimension = item.OrderWidth.ToString() + "x" + (object)item.OrderHeight + "x" + (object)item.OrderLength;
                            data.shipman_name = item.EmployeeName;
                            data.commitment_date = item.CommitmentDate;
                            data.current_address = new FPTCurrentAddress()
                            {
                                district_code = postOffice.DistrictID,
                                province_code = postOffice.ProvinceID,
                                ward_code = "2000000"
                            };
                            var resultFPT = SGPUtils.SendUpdateFPT(data);
                            logger.Info("send update for FPT (Syn PMS): " + new JavaScriptSerializer().Serialize(data) + " |||| " + "result : " + resultFPT);
                        }
                        else if (trackings.MM_Status_StatusID == "13")
                        {
                            item.SGPStatusName = "Chưa phát được";
                            item.DeliveryNote = trackings.MM_MailerDeliveryDetail_DeliveryNotes;
                            item.SGPStatus = new int?(13);
                            this.db.TrackingOrders.Add(new TrackingOrder()
                            {
                                CreateTime = new DateTime?(DateTime.Now),
                                OrderId = new long?((long)item.Id),
                                Status = new int?(5),
                                StatusName = "Chưa phát được",
                                CurrentPost = postOffice.PostOfficeID,
                                DistrictId = postOffice.DistrictID,
                                ProvinceId = postOffice.ProvinceID
                            });
                            this.db.SaveChanges();
                        }
                        else if (trackings.MM_Status_StatusID == "5")
                        {
                            this.db.TrackingOrders.Add(new TrackingOrder()
                            {
                                CreateTime = new DateTime?(DateTime.Now),
                                OrderId = new long?((long)item.Id),
                                Status = new int?(7),
                                StatusName = "Đang phát",
                                CurrentPost = postOffice.PostOfficeID,
                                DistrictId = postOffice.DistrictID,
                                ProvinceId = postOffice.ProvinceID
                            });
                            this.db.SaveChanges();
                        }
                        else if (trackings.MM_Status_StatusID == "7")
                        {
                            this.db.TrackingOrders.Add(new TrackingOrder()
                            {
                                CreateTime = new DateTime?(DateTime.Now),
                                OrderId = new long?((long)item.Id),
                                Status = new int?(6),
                                StatusName = "Chuyển hoàn",
                                CurrentPost = postOffice.PostOfficeID,
                                DistrictId = postOffice.DistrictID,
                                ProvinceId = postOffice.ProvinceID
                            });
                            this.db.SaveChanges();
                        }
                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            return resultInfo;
        }

        [HttpGet]
        [EnableCors("*", "*", "*")]
        public IHttpActionResult GetOrderExcel(string dateFrom, string dateTo, int? status = 0, int? deliveryStatus = 0, string mailerid = "")
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


            //   AspNetUser aspNetUser = db.AspNetUsers.Where(p => p.PostID == "KHDN" && p.CusID == "FPT").FirstOrDefault();
            List<get_mailers_Result> allCG = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", "FPTHCM", "KHDN").ToList<get_mailers_Result>();

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
                        worksheet.Cells[index + 2, 14].Value = allCG[index].ReceiverProvincePMS.Trim();
                        worksheet.Cells[index + 2, 15].Value = allCG[index].ReceiverDistrictPMS.Trim();

                        worksheet.Cells[index + 2, 16].Value = Price;
                        worksheet.Cells[index + 2, 17].Value = ServicePrice;
                        worksheet.Cells[index + 2, 19].Value = Price + ServicePrice;

                        worksheet.Cells[index + 2, 23].Value = 0;

                        worksheet.Cells[index + 2, 24].Value = allCG[index].SenderAddress;
                        worksheet.Cells[index + 2, 25].Value = allCG[index].SenderPhone;
                        worksheet.Cells[index + 2, 29].Value = allCG[index].ReceiverName;
                        worksheet.Cells[index + 2, 30].Value = allCG[index].ReceiverPhone;
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

    }

}
