using System;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Web;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;
using Newtonsoft.Json;

namespace OS_PayNL.DNN.NBrightStore
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class OS_PayNLNotify : IHttpHandler
    {
        private String _lang = "";
        /// <summary>
        /// This function needs to process and returned message from the bank.
        /// This processing may vary widely between banks.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            var modCtrl = new NBrightBuyController();
            var info = modCtrl.GetPluginSinglePageData("OS_PayNLpayment", "OS_PayNLPAYMENT", Utils.GetCurrentCulture());
            var debugMode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
            var debugMsg = "START notifier " + DateTime.Now.ToString("s") + "<br>";

            try
            {
                
                var rtnMsg = "TRUE";
                int OS_PayNLStoreOrderID = 0;
                string apitoken = info.GetXmlProperty("genxml/textbox/apitoken");
                string apitokenstr = info.GetXmlProperty("genxml/textbox/apitokenstr");
                string PAYtransactionId = context.Request.Form["order_id"];
                string OSOrderID = "";

                string getInfoUrl = "https://rest-api.pay.nl/v13/Transaction/info/json/?transactionId=" + PAYtransactionId;
                int paymentState = 0;
                string paymentStateName = "";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = WebRequest.Create(getInfoUrl) as HttpWebRequest;
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
                        InfoResponse r = null;
                        StreamReader Reader = new StreamReader(response.GetResponseStream());
                        r = JsonConvert.DeserializeObject<InfoResponse>(Reader.ReadToEnd());

                        paymentState = r.paymentDetails.state;
                        paymentStateName = r.paymentDetails.stateName;
                        OSOrderID = r.paymentDetails.orderNumber;
                        debugMsg += "paymentState: " + paymentState + "<br>";
                        debugMsg += "paymentStateName: " + paymentStateName.ToString() + "<br>";
                        debugMsg += "OSOrderID: " + paymentState + "<br>";
                    }
                }

                if (paymentState != 0)
                {

                    OS_PayNLStoreOrderID = Convert.ToInt32(OSOrderID);
                    // ------------------------------------------------------------------------
                    //OpenStore                    PAY
                    //010 Incomplete
                    //020 Waiting for Bank
                    //030 Geannuleerd               -90
                    //040 Betaling ok               100
                    //050 Payment Not Verified
                    //060 Wacht op betaling         20
                    //070 Waiting for Stock
                    //080 Gefabriceerd worden
                    //090 Verstuurd
                    //100 Completed
                    //110 Archived
                    //120 Aan het wachten
                    // ------------------------------------------------------------------------

                    debugMsg += "Openstore OrderId: " + OSOrderID + " </br>";
                    debugMsg += "Pay.nl transactionId: " + PAYtransactionId + " </br>";
                    debugMsg += "paymentState: " + paymentState.ToString() + " </br>";


                    var orderData = new OrderData(OS_PayNLStoreOrderID);

                    if (paymentState == 100)
                    {
                        debugMsg += "Order set ok</br>";
                        orderData.PaymentOk();
                    }
                    else if (paymentState == -90)
                    {
                        debugMsg += "Order set 030</br>";
                        orderData.PaymentOk("030");
                    }
                    else
                    {
                        debugMsg += "Order set paymentfail</br>";
                        orderData.PaymentFail();
                    }
                    modCtrl.Update(info);

                }


                if (debugMode)
                {
                    info.SetXmlProperty("genxml/debugmsg", "OS_PayNL Notifier debuginfo:" + debugMsg);
                    modCtrl.Update(info);
                }

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write(rtnMsg);
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.CacheControl = "no-cache";
                HttpContext.Current.Response.Expires = -1;
                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
                
                if (!ex.ToString().StartsWith("System.Threading.ThreadAbortException")) // we expect a thread abort from the End response.
                {
                    if (debugMode)
                    {
                        info.SetXmlProperty("genxml/debugmsg", "OS_PayNL ERROR 130: ("+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + ") " + ex.ToString());
                        modCtrl.Update(info);

                    }
                }
            }


        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }



        public class InfoResponse
        {
            [JsonProperty("paymentDetails")]
            public paymentDetails paymentDetails { get; set; }

        }
        public class paymentDetails
        {
            [JsonProperty("state")]
            public int state { get; set; }

            [JsonProperty("stateName")]
            public string stateName { get; set; }

            [JsonProperty("orderNumber")]
            public string orderNumber { get; set; }
            
        }


    }
}