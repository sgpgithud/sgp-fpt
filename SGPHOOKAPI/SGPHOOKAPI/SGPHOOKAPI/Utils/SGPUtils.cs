using SGPHOOKAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace SGPHOOKAPI.Utils
{
    public class SGPUtils
    {
        public static string sendRequestFPT(string json, string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            UTF8Encoding utF8Encoding = new UTF8Encoding();
            byte[] bytes = utF8Encoding.GetBytes(json);
            httpWebRequest.ContentLength = (long)bytes.Length;
            httpWebRequest.ContentType = "application/json";
            using (Stream requestStream = httpWebRequest.GetRequestStream())
                requestStream.Write(bytes, 0, bytes.Length);
            long num = 0;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    num = response.ContentLength;
                    return new StreamReader(response.GetResponseStream(), (Encoding)utF8Encoding).ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return (string)null;
            }
        }

        public static string SendUpdateFPT(FPTOrderUpdate data)
        {
            data.app_name = "sgp";
            data.password = "UNTnjoonrX";

            return SGPUtils.sendRequestFPT(new JavaScriptSerializer().Serialize(data), "http://shipman.api.ftg.vn/api/v1/shipments/shipment/update_shipment");
        }

        public static string SendUpdateScanFileFPT(FPTUpdateScanFile data)
        {
            data.app_name = "sgp";
            data.password = "UNTnjoonrX";

            return SGPUtils.sendRequestFPT(new JavaScriptSerializer().Serialize(data), "http://shipman.api.ftg.vn/api/v1/shipments/shipment/update_images");
        }

        public static double? CallPrice(int quantity, float weight, string postOfficeId, string proviceId, string customerid, string serviceType, string zoneid)
        {
            PMSSGPDBEntities pMSSGPDB = new PMSSGPDBEntities();

            // tim khoảng cách
            var findDistance = pMSSGPDB.MM_Distances.Where(p => p.PostOfficeID == postOfficeId && p.ProvinceID == proviceId).FirstOrDefault();

            if (findDistance == null)
            {
                return 0;
            }

            // tìm mã bảng giá
            var findPriceMatrix = pMSSGPDB.MM_CallPrice_getMatrix_test(DateTime.Now, postOfficeId, serviceType, customerid, proviceId, (int)findDistance.Distance, "1", "N", zoneid).FirstOrDefault();

            if (findPriceMatrix == null)
            {
                return 0;
            }

            // tim khoang cach
            var findAllPice = pMSSGPDB.MM_CallPrice_Test(findPriceMatrix.PriceMatrixID, DateTime.Now, postOfficeId, serviceType, customerid, proviceId, (int)findDistance.Distance, "1", "N", zoneid).ToList();

            // tinh gia
            double? price = 0;

            if (weight <= 2000)
            {
                var findPrice = findAllPice.Where(p => p.RangeWeightFrom <= weight && p.RangeWeightTo >= weight && p.IsNext == false).FirstOrDefault();

                price = findPrice.Price;


                return price;
           

            }
            else
            {
                var findPrice = findAllPice.Where(p => p.RangeWeightTo == 2000 && p.IsNext == false).FirstOrDefault();

                price = findPrice.Price;

                double? weightTem = weight - 2000;

                var findPriceNext = findAllPice.Where(p => p.IsNext == true).FirstOrDefault();

                double? a = weightTem / findPriceNext.RangeWeightTo;

                price = price + (Math.Round((double)a) * findPriceNext.Price);

                return price;
            }
        }
    }
}