﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GlobeAuction.Controllers
{
    public class IPNController : Controller
    {
        [AllowAnonymous]
        public ActionResult TestPayPalHereIpn()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult TestTicketPurchaseIpn()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public HttpStatusCodeResult Receive(FormCollection form)
        {
            //Store the IPN received from PayPal
            LogRequest(form);

            //Fire and forget verification task
            Task.Run(() => VerifyTask(Request));

            //Reply back a 200 code
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void VerifyTask(HttpRequestBase ipnRequest)
        {
            var verificationResponse = string.Empty;

            try
            {
                var verificationRequest = (HttpWebRequest)WebRequest.Create("https://www.paypal.com/cgi-bin/webscr");

                //Set values for the verification request
                verificationRequest.Method = "POST";
                verificationRequest.ContentType = "application/x-www-form-urlencoded";
                var param = Request.BinaryRead(ipnRequest.ContentLength);
                var strRequest = Encoding.ASCII.GetString(param);

                //Add cmd=_notify-validate to the payload
                strRequest = "cmd=_notify-validate&" + strRequest;
                verificationRequest.ContentLength = strRequest.Length;

                //Attach payload to the verification request
                var streamOut = new StreamWriter(verificationRequest.GetRequestStream(), Encoding.ASCII);
                streamOut.Write(strRequest);
                streamOut.Close();

                //Send the request to PayPal and get the response
                var streamIn = new StreamReader(verificationRequest.GetResponse().GetResponseStream());
                verificationResponse = streamIn.ReadToEnd();
                streamIn.Close();

            }
            catch (Exception exception)
            {
                //Capture exception for manual investigation
                NLog.LogManager.GetCurrentClassLogger().Error(exception, "Unable to process IPN: " + exception.Message);
            }

            ProcessVerificationResponse(verificationResponse);
        }


        private void LogRequest(FormCollection form)
        {
            // Persist the request values into a database or temporary data store
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var msg = "PayPal IPN: " + Environment.NewLine + string.Join(Environment.NewLine, form.AllKeys.Select(k => string.Format("{0}={1}", k, form[k])));
            logger.Info(msg);
        }

        private void ProcessVerificationResponse(string verificationResponse)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("IPN Verify Response = " + verificationResponse);

            if (verificationResponse.Equals("VERIFIED"))
            {
                // check that Payment_status=Completed
                // check that Txn_id has not been previously processed
                // check that Receiver_email is your Primary PayPal email
                // check that Payment_amount/Payment_currency are correct
                // process payment
            }
            else if (verificationResponse.Equals("INVALID"))
            {
                //Log for manual investigation
            }
            else
            {
                //Log error
            }
        }
    }
}