using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DotNetNuke.Common.Utilities;
using Newtonsoft.Json;

namespace OS_PayNL
{
    public class ProviderUtils
    {
        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_PayNLpayment", "OS_PayNLPAYMENT", Utils.GetCurrentCulture());
            return info;
        }

        public static String GetBankRemotePost(OrderData orderData)
        {
        
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_PayNLpayment", "OS_PayNLPAYMENT", orderData.Lang);

            string ip = HttpContext.Current.Request.UserHostAddress;
            string OSorderId = orderData.PurchaseInfo.ItemID.ToString();
            string apitoken = info.GetXmlProperty("genxml/textbox/apitoken");
            string apitokenstr = info.GetXmlProperty("genxml/textbox/apitokenstr");
            string serviceid = info.GetXmlProperty("genxml/textbox/serviceid");
            string returnurl = info.GetXmlProperty("genxml/textbox/returnurl");
            string appliedtotals = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal").ToString("0.00").Replace(",", "").Replace(".", "");
            string testMode = "0";
            if (info.GetXmlPropertyBool("genxml/checkbox/testMode")) { testMode = "1"; }
            

            string notifyUrl = info.GetXmlProperty("genxml/textbox/verifyurl");
            string firstUrl = "https://rest-api.pay.nl/v13/Transaction/start/json?serviceId=" + serviceid + "&amount=" + appliedtotals + "&ipAddress=" + ip + "&finishUrl=" + returnurl + "&testMode="+ testMode + "&transferValue=" + OSorderId + "&transaction[orderExchangeUrl]=" + notifyUrl + "&transaction[orderNumber]=" + OSorderId;
            string redirectToUrl = "";
            string PayTransID = "";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = WebRequest.Create(firstUrl) as HttpWebRequest;
            request.Credentials = new System.Net.NetworkCredential(apitoken, apitokenstr);
            request.Method = WebRequestMethods.Http.Get;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    //return;
                }

                else
                {
                    PaymentResponse r = null;
                    StreamReader Reader = new StreamReader(response.GetResponseStream());
                    r = JsonConvert.DeserializeObject<PaymentResponse>(Reader.ReadToEnd());

                    redirectToUrl = r.Transaction.PaymentUrl;
                    PayTransID = r.Transaction.TransactionID.ToString();
                }
            }





            var rPost = new RemotePost();
                 var param = new string[3];
                 param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                 param[1] = "status=1";
                 var pbxeffectue = Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
                 param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                 param[1] = "status=0";
                 var pbxrefuse = Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
                 var appliedtotal = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal").ToString("0.00").Replace(",", "").Replace(".", ""); ;
                 var postUrl = info.GetXmlProperty("genxml/textbox/mainurl");
            //     //
            //     //if (info.GetXmlPropertyBool("genxml/checkbox/preproduction"))
            //     //{
            //     //    postUrl = info.GetXmlProperty("genxml/textbox/preprodurl");
            //     //}
            //     
                 rPost.Url = redirectToUrl;
            //     rPost.Add("apitoken", info.GetXmlProperty("genxml/textbox/apitoken"));
            //     rPost.Add("serviceid", info.GetXmlProperty("genxml/textbox/serviceid"));

            var rtnStr = "";
            rtnStr = rPost.GetPostHtml();
        
            if (info.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_OS_PaymentGatewaypost.html", rtnStr);
            }
            return rtnStr;
        }


        public class PaymentResponse
        {
            [JsonProperty("status")]
            public Status Status { get; set; }
            [JsonProperty("endUser")]
            public EndUser EndUser { get; set; }
            [JsonProperty("transaction")]
            public TransactionInformation Transaction { get; set; }
        }
        public class Status
        {
            [JsonProperty("result")]
            public int Restult { get; set; }
            [JsonProperty("errorId")]
            public int ErrorId { get; set; }
            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }
        }
        public class EndUser
        {
            public int Blacklist { get; set; }

        }
        public class TransactionInformation
        {
            [JsonProperty("transactionID")]
            public string TransactionID { get; set; }
            [JsonProperty("paymentUrl")]
            public string PaymentUrl { get; set; }
            [JsonProperty("popupAllowed")]
            public int PopupAllowed { get; set; }

        }

    }

}
