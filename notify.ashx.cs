﻿using System;
using System.Runtime.Remoting.Contexts;
using System.Web;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;

namespace OS_Sips.DNN.NBrightStore
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class OS_SipsNotify : IHttpHandler
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
            var info = modCtrl.GetPluginSinglePageData("OS_Sipspayment", "OS_SipsPAYMENT", Utils.GetCurrentCulture());

            try
            {
                var debugMode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
                var debugMsg = "START CALL" + DateTime.Now.ToString("s") + " </br>";
                var rtnMsg = "version=2" + Environment.NewLine + "cdr=1";

                // ------------------------------------------------------------------------
                // In this case the payment provider passes back data via form POST.
                // Get the data we need.
                string returnmessage = "";
                int OS_SipsStoreOrderID = 0;
                string OS_SipsCartID = "";
                string OS_SipsClientLang = "";

                var orderid = Utils.RequestQueryStringParam(context, "ref");
                debugMsg += "orderid: " + orderid + "</br>";

                if (Utils.IsNumeric(orderid))
                {
                    var authcode = Utils.RequestQueryStringParam(context, "auto");
                    var errcode = Utils.RequestQueryStringParam(context, "rtnerr");

                    OS_SipsStoreOrderID = Convert.ToInt32(orderid);
                    // ------------------------------------------------------------------------

                    debugMsg += "OrderId: " + orderid + " </br>";
                    debugMsg += "errcode: " + errcode + " </br>";
                    debugMsg += "authcode: " + authcode + " </br>";

                    var orderData = new OrderData(OS_SipsStoreOrderID);


                    if (authcode == "")
                        rtnMsg = "KO";
                    else
                        rtnMsg = "OK";

                    if (authcode == "")
                    {
                        orderData.PaymentFail();
                    }
                    else
                    {
                        if (errcode == "00000")
                        {
                            orderData.PaymentOk();
                        }
                        else if (errcode == "99999")
                        {
                            orderData.PaymentOk("050");
                        }
                        else
                        {
                            orderData.PaymentFail();
                        }
                    }
                }
                if (debugMode)
                {
                    debugMsg += "Return Message: " + rtnMsg;
                    info.SetXmlProperty("genxml/debugmsg", debugMsg);
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
                    info.SetXmlProperty("genxml/debugmsg", "OS_Sips ERROR: " + ex.ToString());
                    modCtrl.Update(info);
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


    }
}