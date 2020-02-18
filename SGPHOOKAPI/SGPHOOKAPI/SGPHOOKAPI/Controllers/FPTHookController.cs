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

                var sender = "FPTHCM";
                var postOffice = "KHDN";
                var checkVendor = db.FPTVendorCodes.Where(p => p.Id == paser.sender_address.address_code).FirstOrDefault();

                if (checkVendor != null)
                {
                    if (checkVendor.SGPPostOffice == "KV04")
                    {
                        sender = "FPTCTO";
                        postOffice = "CTHO";
                    }
                    else if (checkVendor.SGPPostOffice == "KV03")
                    {
                        sender = "FPTKV03";
                        postOffice = "KV03";
                    }
                }

                var order = new MailerInfo()
                {
                    CreateDate = DateTime.Now,
                    CurrentStatus = 0,
                    CustomerId = sender,
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
                    PostID = postOffice,
                    CurrentPost = postOffice,
                    SendId = sender,
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

                PostOffice postOfficeFind = db.PostOffices.Where(p => p.PostOfficeID == postOffice).FirstOrDefault();

                this.db.TrackingOrders.Add(new TrackingOrder()
                {
                    CreateTime = new DateTime?(DateTime.Now),
                    OrderId = order.Id,
                    Status = order.CurrentStatus,
                    StatusName = "Đã tạo đơn hàng",
                    CurrentPost = postOfficeFind.PostOfficeID,
                    DistrictId = postOfficeFind.DistrictID,
                    ProvinceId = postOfficeFind.ProvinceID
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
            logger.Info("cancel order...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0
            };
            try
            {
                var requestContent = Request.Content.ReadAsStringAsync().Result;
                var jsonserializer = new JavaScriptSerializer();
                logger.Info(requestContent);

                var paser = jsonserializer.Deserialize<OrderRequireRequest>(requestContent);

                if (this.AuthLogin(paser.token_key) == null)
                    throw new Exception("Wrong authorication !!!");

                MailerInfo find = db.MailerInfoes.Where(p => p.OrderCode == paser.order_number).FirstOrDefault();

                if (find == null)
                    throw new Exception("Không tìm thấy thông tin");

                if (find.CurrentStatus == 3)
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
                logger.Error(ex.Message);
                resultInfo.error = 1;
                resultInfo.message = ex.Message;
            }

            return resultInfo;
        }

        [HttpPost]
        public ResultInfo GetOrderFPTInfoAsync()
        {
            logger.Info("Send order...");
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
                logger.Info(result);
                OrderRequireRequest paser = scriptSerializer.Deserialize<OrderRequireRequest>(result);
                if (this.AuthLogin(paser.token_key) == null)

                    throw new Exception("Wrong authorication !!!");
                List<get_mailer_fpt_Result> listOrders = db.get_mailer_fpt("%" + paser.order_number + "%").ToList();

                resultInfo.data = listOrders;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                resultInfo.error = 1;
                resultInfo.message = ex.Message;
            }
            return resultInfo;
        }

        [HttpPost]
        public ResultInfo TrackingOrderFPTAsync()
        {
            logger.Info("Send order...");
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
                logger.Info(result);
                var paser = scriptSerializer.Deserialize<OrderRequireRequest>(result);

                if (this.AuthLogin(paser.token_key) == null)
                    throw new Exception("Wrong authorication !!!");

                var find = db.get_mailer_fpt(paser.order_number).FirstOrDefault();

                if (find == null)
                    throw new Exception("Không tìm thấy đơn hàng");

                var orderedEnumerable = db.TrackingOrders.Where(p => p.OrderId == find.Id).OrderBy(p => p.CreateTime).ToList();

                List<OrderTrackingResult> orderTrackingResultList = new List<OrderTrackingResult>();

                string status_date = "";
                int status = 0;

                foreach (TrackingOrder trackingOrder in orderedEnumerable)
                {

                    status_date = trackingOrder.CreateTime.Value.ToString("dd/MM/yyyy HH:mm");
                    status = trackingOrder.Status.Value;


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
                }

                resultInfo.data = new
                {

                    infos = new
                    {
                        commitment_date = find.CommitmentDate,
                        status = status,
                        status_date = status_date,
                        order_number = find.OrderCode,
                        total_cost = (find.Price + find.ServicePrice) * 1000 + "",
                        package_weight = find.OrderWeight / 1000,
                        shipman_name = find.EmployeeName,
                        delivery_note = find.DeliveryNote,
                        delivery_to = find.DeliveryTo,
                        delivery_date = find.DeliveryDate
                    },
                    tracking = orderTrackingResultList

                };
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
            logger.Info("Send GetOrderFPTByStatus...");

            if (String.IsNullOrEmpty(mailerid))
            {
                mailerid = "";
            }

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

            var findSendCode = db.CustomerPosts.Where(p => p.PostID == findUser.PostID && p.GroupID == "FPT").FirstOrDefault();

            List<get_mailers_Result> findMailers = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", findSendCode.CustomerID, findSendCode.PostID).Where(p => p.CurrentStatus == status).ToList();

            if (status == 3)
            {
                // Đang nhân
                if (deliveryStatus != 0)
                {

                    if (deliveryStatus == 15)
                    {
                        findMailers = findMailers.Where(p => p.SGPStatus != 6).ToList();
                    }
                    else
                    {
                        findMailers = findMailers.Where(p => p.SGPStatus == deliveryStatus).ToList();
                    }

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
            logger.Info("Send update order...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0
            };
            try
            {
                string result = Request.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
                logger.Info(result);
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
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://sheetdb.io/api/v1/ot7b3ypwk9ri8");
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
                        if (synDataInfo != null && synDataInfo.Status == "Đã nhận hàng")
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
                            var fee = synDataInfo.Fee.Replace(".", "");
                            fee = fee.Trim();
                            item.Price = Convert.ToDouble(fee);
                            item.OrderService = synDataInfo.DV;
                            item.PaymentType = synDataInfo.HTTT;
                            item.OrderType = synDataInfo.LH;
                            item.OrderQuality = string.IsNullOrEmpty(synDataInfo.SL) ? 1 : Convert.ToInt32(synDataInfo.SL);
                            item.OrderWeight = new double?(Convert.ToDouble(synDataInfo.TL));
                            item.CommitmentDate = str1;
                            item.ReceiverAddress = synDataInfo.Dia_chi_phat;
                            item.ReceiverProvincePMS = synDataInfo.Code;
                            item.ReceiverDistrictPMS = synDataInfo.Quan_pms;
                            item.ServicePrice = Convert.ToDouble(string.IsNullOrEmpty(synDataInfo.Phu_phi) ? "0" : synDataInfo.Phu_phi);
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
                            //FPTOrderUpdate fptOrderUpdate = data;
                            double? orderWeight = item.OrderWeight;
                            double num2 = 1000.0;
                            string str2 = orderWeight != null ? (Convert.ToDouble(orderWeight.GetValueOrDefault()) / num2) + "" : "0";
                            //  fptOrderUpdate.package_weight = str2;
                            data.receiver_name = item.ReceiverName;
                            data.cod = item.COD + "";
                            data.total_cost = (item.Price + item.ServicePrice) * 1000 + "";
                            data.dimension = item.OrderWidth.ToString() + "x" + (object)item.OrderHeight + "x" + (object)item.OrderLength;
                            data.shipman_name = "";
                            data.commitment_date = item.CommitmentDate;
                            data.current_address = new FPTCurrentAddress()
                            {
                                district_code = "00",
                                province_code = "00",
                                ward_code = "00"
                            };
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

                var findUser = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).FirstOrDefault();

                var findSendCode = db.CustomerPosts.Where(p => p.PostID == findUser.PostID && p.GroupID == "FPT").FirstOrDefault();

                double? calPrice = 0;

                try
                {
                    calPrice = SGPUtils.CallPrice(paser.quantity, paser.weight, findSendCode.PostID, paser.provincePMS.Trim(), findSendCode.PMSCusID, paser.orderService, findSendCode.ZoneID);
                }
                catch
                {
                    calPrice = 0;
                }

                findMailer.Price = calPrice;


                findMailer.ServicePrice = paser.priceService;
                findMailer.CurrentStatus = 3;
                findMailer.ReceiverProvincePMS = paser.provincePMS.Trim();
                findMailer.OrderWeight = paser.weight;
                findMailer.OrderType = paser.orderType;
                findMailer.OrderService = paser.orderService;
                findMailer.OrderQuality = paser.quantity;
                findMailer.MailerID = paser.mailerid;
                findMailer.OrderNote = paser.notes;
                findMailer.TakeDate = DateTime.Now;
                findMailer.CommitmentDate = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd HH:mm:ss");

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

                FPTOrderUpdate data = new FPTOrderUpdate();
                data.state = 2;
                data.operation = "Confirmed";
                data.no_pack = string.Concat((object)findMailer.OrderQuality);
                data.note = "Confirmed pick up";
                data.order_number = findMailer.OrderCode;
                data.status_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //FPTOrderUpdate fptOrderUpdate = data;
                double? orderWeight = findMailer.OrderWeight;
                double num2 = 1000.0;
                string str2 = orderWeight != null ? (Convert.ToDouble(orderWeight.GetValueOrDefault()) / num2) + "" : "0";
                //  fptOrderUpdate.package_weight = str2;
                data.receiver_name = findMailer.ReceiverName;
                data.cod = findMailer.COD + "";
                data.total_cost = (findMailer.Price + findMailer.ServicePrice) * 1000 + "";
                data.dimension = findMailer.OrderWidth.ToString() + "x" + (object)findMailer.OrderHeight + "x" + (object)findMailer.OrderLength;
                data.shipman_name = "";
                data.commitment_date = findMailer.CommitmentDate;
                data.current_address = new FPTCurrentAddress()
                {
                    district_code = "00",
                    province_code = "00",
                    ward_code = "00"
                };
                // log
                var resultFPT = SGPUtils.SendUpdateFPT(data);

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
        public ResultInfo SendFPTUpdate()
        {
            var allMailers = db.MailerInfoes.Where(p => p.CurrentStatus == 3).ToList();

            foreach (var item in allMailers)
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
        public ResultInfo SendFPTUpdateByMailer(String mailer)
        {
            var item = db.MailerInfoes.Where(p => p.MailerID == mailer).FirstOrDefault();

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
                            num2 = 3;
                            break;
                    }
                    if (num2 == -1)
                    {
                        resultInfo.error = 1;
                        return resultInfo;
                    }
                    findOrder.CurrentStatus = new int?(num2);
                    findOrder.StatusNotes = num2 == 3 ? "Đã nhận đơn hàng" : "Đã hủy đơn hàng";
                    findOrder.ModifyDate = new DateTime?(DateTime.Now);
                    db.Entry(findOrder).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                    PostOffice postOffice = db.PostOffices.Where(p => p.PostOfficeID == findOrder.CurrentPost).FirstOrDefault();

                    this.db.TrackingOrders.Add(new TrackingOrder()
                    {
                        CreateTime = new DateTime?(DateTime.Now),
                        OrderId = new long?((long)findOrder.Id),
                        Status = findOrder.CurrentStatus,
                        StatusName = num2 == 3 ? "Đã nhận đơn hàng" : "Đã hủy đơn hàng",
                        CurrentPost = postOffice.PostOfficeID,
                        DistrictId = postOffice.DistrictID,
                        ProvinceId = postOffice.ProvinceID
                    });

                    this.db.SaveChanges();
                    /*
                    if (num2 == 2)
                        resultInfo.data = (object)findOrder;
                        */
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
            logger.Info("Send SynPMS...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                message = "Đã thêm"
            };

            List<MailerInfo> list = db.MailerInfoes.Where(p => p.CurrentStatus == 3 && p.SGPStatus != 6 && p.UserSend == "fpt@spt.vn").ToList();

            SGPServiceClient sgpServiceClient = new SGPServiceClient();
            foreach (MailerInfo mailerInfo1 in list)
            {
                MailerInfo item = mailerInfo1;
                try
                {
                    Trackings trackings = sgpServiceClient.ToolTracking(item.MailerID).FirstOrDefault<Trackings>();
                    if (trackings != null)
                    {

                        item.EmployeeId = trackings.BS_Employees_EmployeeID;
                        item.EmployeeName = trackings.BS_Employees_EmployeeName;
                        item.CurrentPost = trackings.MM_Mailers_CurrentPostOfficeID;
                        item.DeliveryDate = trackings.MM_MailerDeliveryDetail_DeliveryDate != null ? trackings.MM_MailerDeliveryDetail_DeliveryDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                        item.SGPStatus = Convert.ToInt32(trackings.MM_Status_StatusID);
                        item.SGPStatusName = trackings.MM_Status_StatusName;
                        item.DeliveryTo = trackings.MM_MailerDeliveryDetail_DeliveryTo;
                        item.DeliveryNote = trackings.MM_MailerDeliveryDetail_DeliveryNotes;

                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        PostOffice postOffice = db.PostOffices.Where(p => p.PostOfficeID == item.CurrentPost).FirstOrDefault();

                        var findProvince = db.Provinces.Where(p => p.ProvincePMS == postOffice.ProvinceID).FirstOrDefault();
                        var findDistrict = db.Districts.Where(p => p.DistrictPMS == postOffice.DistrictID).FirstOrDefault();

                        FPTOrderUpdate data = new FPTOrderUpdate();
                        data.state = 6;
                        data.operation = "Delivery";
                        data.no_pack = item.OrderQuality + "";
                        data.note = "Đang phát";
                        data.order_number = item.OrderCode;

                        data.status_date = trackings.MM_Mailers_AcceptDate != null ? trackings.MM_Mailers_AcceptDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        data.package_weight = (item.OrderWeight / 1000.0) + "";

                        data.receiver_name = item.DeliveryTo;

                        data.cod = item.COD + "";
                        data.total_cost = (item.Price + item.ServicePrice) * 1000 + "";

                        data.dimension = item.OrderWidth + "x" + item.OrderHeight + "x" + item.OrderLength;

                        data.shipman_name = item.EmployeeName;
                        data.commitment_date = item.CommitmentDate;

                        data.current_address = new FPTCurrentAddress()
                        {
                            district_code = findDistrict != null ? findDistrict.Name : "",
                            province_code = findProvince != null ? findProvince.Name : "",
                            ward_code = "2000000"
                        };

                        if (trackings.MM_Status_StatusID == "6")
                        {
                            data.state = 4;
                            data.operation = "Success";
                            data.status_date = item.DeliveryDate;

                            this.db.TrackingOrders.Add(new TrackingOrder()
                            {
                                CreateTime = trackings.MM_MailerDeliveryDetail_DeliveryDate,
                                OrderId = item.Id,
                                Status = new int?(4),
                                StatusName = "Đã phát thành công",
                                CurrentPost = postOffice.PostOfficeID,
                                DistrictId = postOffice.DistrictID,
                                ProvinceId = postOffice.ProvinceID
                            });
                            this.db.SaveChanges();

                        }
                        else if (trackings.MM_Status_StatusID == "13")
                        {
                            data.state = 5;
                            data.operation = "Fail";
                        }
                        else if (trackings.MM_Status_StatusID == "7")
                        {
                            data.status_date = item.DeliveryDate;
                            data.state = 5;
                            data.operation = "Fail";
                        }

                        var resultFPT = SGPUtils.SendUpdateFPT(data);

                        logger.Info("send update for FPT (Syn PMS): " + new JavaScriptSerializer().Serialize(data) + " |||| " + "result : " + resultFPT);

                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            return resultInfo;
        }


        [HttpGet]
        [EnableCors("*", "*", "*")]
        public ResultInfo SendScanFile()
        {
            logger.Info("Send SynPMS...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                message = "Đã thêm"
            };

            var allMailers = db.MailerInfoes.Where(p => p.CustomerId == "FPTHCM" && p.SGPStatus == 6 && p.SendAttachFilePartner != true).ToList();
            var storageDB = new sptstorageEntities();
            foreach (var item in allMailers)
            {
                var document = storageDB.Documents.Where(p => p.CGNumber.StartsWith(item.MailerID)).ToList().Take(3);

                List<String> scanFiles = new List<string>();

                foreach (var doc in document)
                {
                    scanFiles.Add("http://ftp.saigonpost.vn/" + doc.FolderPath);
                }

                if (scanFiles.Count > 0)
                {
                    SGPUtils.SendUpdateScanFileFPT(new FPTUpdateScanFile()
                    {
                        images = scanFiles,
                        order_number = item.OrderCode
                    });

                    item.SendAttachFilePartner = true;
                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }

            return resultInfo;

        }


        [HttpGet]
        [EnableCors("*", "*", "*")]
        public IHttpActionResult GetOrderExcel(string dateFrom, string dateTo, int? status = 0, int? deliveryStatus = 0, string mailerid = "", string postId = "KHDN")
        {

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


            var findSendCode = db.CustomerPosts.Where(p => p.PostID == postId && p.GroupID == "FPT").FirstOrDefault();

            List<get_mailers_Result> allCG = db.get_mailers(dateFrom, dateTo, "%" + mailerid + "%", findSendCode.CustomerID, findSendCode.PostID).Where(p => p.CurrentStatus == status).ToList();


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
                        worksheet.Cells[index + 2, 14].Value = allCG[index].ReceiverProvincePMS != null ? allCG[index].ReceiverProvincePMS.Trim() : "";
                        worksheet.Cells[index + 2, 15].Value = allCG[index].ReceiverDistrictPMS != null ? allCG[index].ReceiverDistrictPMS.Trim() : "";

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



        [HttpGet]
        public ResultInfo FindOrder(string mailerid, string cusId)
        {
            logger.Info("Send GetOrderFPTByStatus...");
            ResultInfo resultInfo = new ResultInfo()
            {
                error = 0,
                data = new List<MailerInfo>()
            };


            var itemes = db.MailerInfoes.Where(p => p.MailerID.Contains(mailerid) && p.SendId == cusId && p.CurrentStatus == 3).ToList();

            List<MailerRpt> mailer = new List<MailerRpt>();

            foreach (var item in itemes)
            {
                mailer.Add(new MailerRpt()
                {
                    MailerID = item.MailerID,
                    HH = item.OrderType == "HH" ? "X" : "",
                    TL = item.OrderType == "TL" ? "X" : "",
                    OrderContent = item.OrderNote,
                    OrderCode = "*" + item.OrderCode + "*",
                    ReceiverAddress = item.ReceiverContactName + "-" + item.ReceiverName + "-" + item.ReceiverAddress,
                    SendAddress = item.SenderContactName + "-" + item.SenderName + "-" + item.SenderAddress,
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
            }

            resultInfo.data = mailer;

            return resultInfo;
        }

    }

}
