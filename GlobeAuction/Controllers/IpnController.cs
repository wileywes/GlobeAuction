using GlobeAuction.Helpers;
using GlobeAuction.Models;
using System;
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
        private ApplicationDbContext db = new ApplicationDbContext();
        private NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

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
            var ppTrans = new PayPalTransaction(form);

            //Store the IPN received from PayPal
            LogRequest(ppTrans);
            
            VerifyTask(Request, ppTrans);

            //Reply back a 200 code
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void VerifyTask(HttpRequestBase ipnRequest, PayPalTransaction ppTrans)
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
                _logger.Error(exception, "Unable to process IPN: " + exception.Message);
            }

            ProcessVerificationResponse(verificationResponse, ppTrans);
        }


        private void LogRequest(PayPalTransaction ppTrans)
        {
            // Persist the request values into a database or temporary data store
            //var logger = _logger;
            //var msg = "PayPal IPN: " + Environment.NewLine + string.Join(Environment.NewLine, form.AllKeys.Select(k => string.Format("{0}={1}", k, form[k])));
            //logger.Info(msg);
            
            db.PayPalTransactions.Add(ppTrans);
            db.SaveChanges(); //go ahead and record the transaction
        }

        private void ProcessVerificationResponse(string verificationResponse, PayPalTransaction ppTrans)
        {
            try
            {
                _logger.Info("IPN Verify Response = " + verificationResponse);

                if (verificationResponse.Equals("VERIFIED"))
                {
                    // check that Payment_status=Completed
                    // check that Txn_id has not been previously processed
                    // check that Receiver_email is your Primary PayPal email
                    // check that Payment_amount/Payment_currency are correct
                    // process payment
                    if (ppTrans.TransactionType == PayPalTransactionType.BidderCart)
                    {
                        var bidderId = BidderRepository.GetBidderIdFromTransaction(ppTrans);
                        if (!bidderId.HasValue)
                        {
                            _logger.Error("Unable to find bidder ID from PP Trans Id {0}", ppTrans.TxnId);
                            return;
                        }

                        Bidder bidder = db.Bidders.Find(bidderId.Value);
                        if (bidder == null)
                        {
                            _logger.Error("Unable to find bidder ID {1} for PP Trans Id {0}", ppTrans.TxnId, bidderId.Value);
                            return;
                        }

                        new BidderRepository(db).ApplyTicketPaymentToBidder(ppTrans, bidder);
                        _logger.Info("Updated payment for bidder ID {0} via IPN", bidderId.Value);
                    }
                }
                else if (verificationResponse.Equals("INVALID"))
                {
                    _logger.Error("Invalid Ipn in PP Trans ID {0}", ppTrans.TxnId);
                }
                else
                {
                    _logger.Error("Unrecognized validation response [{0}] for PP Trans ID {1}", verificationResponse, ppTrans.TxnId);
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Unable to process IPN response [{0}] for PP Trans ID {1}: {2}", verificationResponse, ppTrans.TxnId, exc.ToString());
            }
        }
    }
}