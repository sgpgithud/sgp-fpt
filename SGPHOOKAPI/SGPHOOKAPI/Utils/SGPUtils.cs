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
        public static string sendRequestFPT(string json)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://shipman.api.ftg.vn/api/v1/shipments/shipment/update_shipment");
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
            data.current_address = new FPTCurrentAddress()
            {
                district_code = "00",
                province_code = "00",
                ward_code = "00"
            };
            return SGPUtils.sendRequestFPT(new JavaScriptSerializer().Serialize(data));
        }
    }
}