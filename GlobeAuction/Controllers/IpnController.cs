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

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult TestPayPalHereIpn()
        {
            return View();
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult TestTicketPurchaseIpn()
        {
            return View();
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult TestIpnFromPayPalQs()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateInput(false)]
        public HttpStatusCodeResult Receive(FormCollection form)
        {
            if (ShouldIgnoreIpn(form))
            {
                VerifyIpn(Request);
                _logger.Info("Ignored IPN " + form["ipn_track_id"]);
            }
            else
            {
                var ppTrans = new PayPalTransaction(form);
                
                //Store the IPN received from PayPal
                LogRequest(ppTrans);

                var verificationResponse = VerifyIpn(Request);
                ProcessVerificationResponse(verificationResponse, ppTrans);
            }

            //Reply back a 200 code
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private bool ShouldIgnoreIpn(FormCollection form)
        {
            if (form["txn_type"] == "new_case" && form["case_type"] == "dispute")
                return true;

            return false;
        }

        private string VerifyIpn(HttpRequestBase ipnRequest)
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

            return verificationResponse;
        }

        private void LogRequest(PayPalTransaction ppTrans)
        {
            // Persist the request values into a database or temporary data store            
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
                    //leave if the payment wasn't successful
                    if (!ppTrans.WasPaymentSuccessful) return;

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

                        var invoiceRepos = new InvoiceRepository(db);
                        var regInvoice = invoiceRepos.GetRegistrationInvoiceForBidder(bidder);
                        invoiceRepos.ApplyPaymentToInvoice(ppTrans, regInvoice);

                        _logger.Info("Updated payment for bidder ID {0} via IPN", bidderId.Value);
                    }
                    else if (ppTrans.TransactionType == PayPalTransactionType.InvoiceCart)
                    {
                        var invoiceId = InvoiceRepository.GetInvoiceIdFromTransaction(ppTrans);
                        if (!invoiceId.HasValue)
                        {
                            _logger.Error("Unable to find invoice ID from PP Trans Id {0}", ppTrans.TxnId);
                            return;
                        }

                        var invoice = db.Invoices.Find(invoiceId.Value);
                        if (invoice == null)
                        {
                            _logger.Error("Unable to find invoice ID {1} for PP Trans Id {0}", ppTrans.TxnId, invoiceId.Value);
                            return;
                        }

                        new InvoiceRepository(db).ApplyPaymentToInvoice(ppTrans, invoice);
                        _logger.Info("Updated payment for invoice ID {0} via IPN", invoiceId.Value);
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