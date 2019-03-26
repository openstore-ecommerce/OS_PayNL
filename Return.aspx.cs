// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Admin;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using System.Web.UI;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace OS_PayNL
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Return : CDefault
    {
        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {


            try
            {
                var modCtrl = new NBrightBuyController();
                var info = modCtrl.GetPluginSinglePageData("OS_PayNLpayment", "OS_PayNLPAYMENT", Utils.GetCurrentCulture());

                int OS_PayNLStoreOrderID = 0;
                string apitoken = info.GetXmlProperty("genxml/textbox/apitoken");
                string apitokenstr = info.GetXmlProperty("genxml/textbox/apitokenstr");
                string PAYtransactionId = Request.QueryString["orderId"];
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
                    }
                }

                if (paymentState != 0)
                {
                    var param = "?orderid=" + OSOrderID;

                    OS_PayNLStoreOrderID = Convert.ToInt32(OSOrderID);

                    var orderData = new OrderData(OS_PayNLStoreOrderID);

                    if (paymentState == 100)
                    {
                        param += "&status=1";
                    }
                    else
                    {
                        param += "&status=0";
                    }

                    string finishUrl = DotNetNuke.Common.Globals.NavigateURL(StoreSettings.Current.PaymentTabId); 
                    Response.Redirect(finishUrl + param);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        #endregion



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