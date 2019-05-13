using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using GlobeAuction.Models;
using GlobeAuction.Helpers;
using Microsoft.AspNet.Identity;

namespace GlobeAuction.Controllers
{
    [AllowAnonymous]
    public class InvoicesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invoices
        [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
        public ActionResult Index()
        {
            var invoices = db.Invoices
                .Include(i => i.Bids)
                .Include(i => i.Bidder)
                .Include("Bidder.AuctionGuests")
                .Include(i => i.PaymentTransaction)
                .Include(i => i.StoreItemPurchases)
                .Include("StoreItemPurchases.StoreItem")
                .ToList();
            var viewModels = invoices.Select(i => new InvoiceListViewModel(i)).ToList();
            return View(viewModels);
        }

        // GET: Invoices/Details/5
        [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        public ActionResult Checkout()
        {
            return View(new BidderLookupModel());
        }

        // POST: AuctionItems/Delete/5
        [HttpPost, ActionName("Checkout")]
        [ValidateAntiForgeryToken]
        public ActionResult CheckoutConfirmed(BidderLookupModel lookupModel)
        {
            var requireEmailMatch = true;

            if (Request.IsAuthenticated && User.IsInRole(AuctionRoles.CanCheckoutWinners))
            {
                requireEmailMatch = false;
            }

            if (ModelState.IsValid)
            {
                var bidder = db.Bidders.FirstOrDefault(b =>
                    b.IsDeleted == false &&
                    b.BidderNumber == lookupModel.BidderNumber &&
                    b.LastName.Equals(lookupModel.LastName, StringComparison.OrdinalIgnoreCase) &&
                    (!requireEmailMatch || b.Email.Equals(lookupModel.Email, StringComparison.OrdinalIgnoreCase)));

                if (bidder == null)
                {
                    ModelState.AddModelError("bidderNumber", "No bidder was found matching this information.");
                }
                else
                {
                    //send to review page once we've verified who they are
                    return RedirectToAction("ReviewBidderWinnings", new { bid = bidder.BidderId, email = bidder.Email });
                }
            }

            return View(lookupModel);
        }

        public ActionResult ReviewBidderWinnings(int bid, string email, bool? manualPaidSuccessful, bool? selfPaySuccessful)
        {
            //check they are who they say they are
            var bidder = db.Bidders.FirstOrDefault(b => b.IsDeleted == false && b.BidderId == bid && b.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (bidder == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var invoicesForBidder = db.Invoices
                .Include(i => i.Bids)
                .Include(i => i.PaymentTransaction)
                .Include(i => i.StoreItemPurchases)
                .Where(i => i.Bidder.BidderId == bidder.BidderId && i.InvoiceType == InvoiceType.AuctionCheckout)
                .ToList();

            var auctionWinningsForBidderNotInInvoice = db.Bids
                .Include(a => a.AuctionItem)
                .Include("AuctionItem.DonationItems")
                .Where(a => a.Bidder.BidderId == bidder.BidderId && a.Invoice == null && a.IsWinning)
                .ToList();

            var storeItems = db.StoreItems.Where(s => s.CanPurchaseInAuctionCheckout && s.IsDeleted == false).ToList();
            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanAdminUsers))
            {
                //remove admin-only store items
                storeItems = storeItems.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }
            var storeItemsAvailableToPurchase = storeItems
                .Where(si => !si.IsRaffleTicket && (si.Quantity > 0 || si.HasUnlimitedQuantity))
                .OrderBy(si => si.Price)
                .Select(si => new BuyItemViewModel(si))
                .ToList();

            var viewModel = new ReviewBidderWinningsViewModel(bidder, invoicesForBidder, auctionWinningsForBidderNotInInvoice, storeItemsAvailableToPurchase);

            viewModel.ShowManuallyPaidSuccess = manualPaidSuccessful.GetValueOrDefault(false);
            viewModel.ShowSelfPaySuccess = selfPaySuccessful.GetValueOrDefault(false);

            return View(viewModel);
        }

        [HttpPost, ActionName("ReviewBidderWinnings")]
        [ValidateAntiForgeryToken]
        public ActionResult ReviewBidderWinningsPayNow(ReviewBidderWinningsViewModel model, string submitButton)
        {
            var bidder = db.Bidders.FirstOrDefault(b => b.IsDeleted == false && b.BidderId == model.BidderId && b.Email.Equals(model.BidderEmail, StringComparison.OrdinalIgnoreCase));
            if (bidder == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var auctionItemIds = model.AuctionItemsNotInInvoice.Select(i => i.AuctionItem.AuctionItemId).ToList();
            var winnings = db.Bids
                .Include(b => b.AuctionItem)
                .Where(bid => auctionItemIds.Contains(bid.AuctionItem.AuctionItemId) &&
                    bid.Bidder.BidderId == bidder.BidderId &&
                    bid.Invoice == null && bid.IsWinning)
                    .ToList();

            if (!winnings.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var storeItemPurchases = new ItemsRepository(db).GetStorePurchasesWithIndividualizedRaffleTickets(model.ItemPurchases);

            PaymentMethod? manualPayMethod = null;
            if (submitButton.StartsWith("Invoice and Mark Paid"))
            {
                if (submitButton.EndsWith("(Cash)")) manualPayMethod = PaymentMethod.Cash;
                if (submitButton.EndsWith("(Check)")) manualPayMethod = PaymentMethod.Check;
                if (submitButton.EndsWith("(PayPal)")) manualPayMethod = PaymentMethod.PayPalHere;
            }

            var invoice = new InvoiceRepository(db).CreateInvoiceForAuctionItems(bidder, winnings, storeItemPurchases,
                manualPayMethod, manualPayMethod.HasValue ? User.Identity.GetUserName() : bidder.Email);

            if (manualPayMethod.HasValue)
            {
                return RedirectToAction("ReviewBidderWinnings", new { bid = invoice.Bidder.BidderId, email = invoice.Bidder.Email, manualPaidSuccessful = true });
            }

            return RedirectToAction("RedirectToPayPal", new { iid = invoice.InvoiceId, email = invoice.Email });
        }

        [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
        public ActionResult RemoveBidFromUnpaidInvoice(int invoiceId, int bidId)
        {
            return RemoveBidFromInvoice(invoiceId, bidId, false);
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult RemoveBidFromPaidInvoice(int invoiceId, int bidId)
        {
            return RemoveBidFromInvoice(invoiceId, bidId, true);
        }

        private ActionResult RemoveBidFromInvoice(int invoiceId, int bidId, bool invoiceShouldBeCurrentlyPaid)
        {
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null) return HttpNotFound();
            if (invoiceShouldBeCurrentlyPaid && !invoice.IsPaid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (!invoiceShouldBeCurrentlyPaid && invoice.IsPaid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //grab a copy before we delete the invoice
            var bidderId = invoice.Bidder.BidderId;
            var bidderNumber = invoice.Bidder.BidderNumber;
            var bidderEmail = invoice.Bidder.Email;
            var bidderName = invoice.Bidder.FirstName + " " + invoice.Bidder.LastName;

            var item = db.Bids.FirstOrDefault(ai => ai.Invoice != null && ai.Invoice.InvoiceId == invoiceId && ai.BidId == bidId && ai.Bidder.BidderId == bidderId);
            if (item == null)
            {
                return HttpNotFound();
            }

            item.Invoice = null;
            invoice.Bids.Remove(invoice.Bids.First(ai => ai.BidId == bidId));

            //delete the invoice entirely if there are no more lines on it
            if (invoice.Bids.Count == 0 && invoice.StoreItemPurchases.Count == 0 && invoice.TicketPurchases.Count == 0)
            {
                db.Invoices.Remove(invoice);
            }

            db.SaveChanges();

            if (invoiceShouldBeCurrentlyPaid)
            {
                //paid invoices send an email to refund later
                var body = "The following item was removed from a paid invoice.  The winner likely needs to be refunded via PayPal:<br/><br/>" +
                    $"<b>Invoice #:</b> {invoice.InvoiceId}<br />" +
                    $"<b>Bidder #:</b> {bidderNumber}<br />" +
                    $"<b>Bidder Name:</b> {bidderNumber}<br />" +
                    $"<b>Bidder Email:</b> {bidderName}<br />" +
                    $"<b>Item #:</b> {item.AuctionItem.UniqueItemNumber}<br />" +
                    $"<b>Amount Paid:</b> {item.BidAmount:C}<br />" +
                    $"<b>Payment Method:</b> {invoice.PaymentMethod}<br />" +
                    $"<b>Invoice Type:</b> {invoice.InvoiceType}<br />";

                new EmailHelper().SendEmail("auction@theglobeacademy.net", "Paid Item Removed from Invoice - Refund Needed", body);
            }

            return RedirectToAction("ReviewBidderWinnings", new { bid = bidderId, email = bidderEmail });
        }

        [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
        public ActionResult RemoveStoreItemFromUnpaidInvoice(int invoiceId, int sipId)
        {
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null) return HttpNotFound();
            if (invoice.IsPaid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //grab a copy before we delete the invoice
            var bidId = invoice.Bidder.BidderId;
            var bidEmail = invoice.Bidder.Email;

            var item = invoice.StoreItemPurchases.FirstOrDefault(sip => sip.StoreItemPurchaseId == sipId);
            if (item == null)
            {
                return HttpNotFound();
            }

            //add the quantity back if it's not a raffle ticket
            if (item.StoreItem.IsRaffleTicket == false)
            {
                item.StoreItem.Quantity += item.Quantity;
            }

            db.StoreItemPurchases.Remove(item);

            //delete the invoice entirely if there are no more lines on it
            if (invoice.Bids.Count == 0 && invoice.StoreItemPurchases.Count == 0 && invoice.TicketPurchases.Count == 0)
            {
                db.Invoices.Remove(invoice);
            }

            db.SaveChanges();

            return RedirectToAction("ReviewBidderWinnings", new { bid = bidId, email = bidEmail });
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult MarkInvoicePaid(int invoiceId, int ppTransId)
        {
            var ppTrans = db.PayPalTransactions.Find(ppTransId);
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null || ppTrans == null)
            {
                return HttpNotFound();
            }

            new InvoiceRepository(db).ApplyPaymentToInvoice(ppTrans, invoice);
            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for invoice  " + invoice.InvoiceId + " manually via MarkInvoicerPaid");

            return RedirectToAction("Index", "Invoices");
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult MarkInvoicePaidManually(int invoiceId)
        {
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null || invoice.IsPaid) return HttpNotFound();

            var invoiceRepos = new InvoiceRepository(db);
            invoiceRepos.ApplyPotentialManualPayment(invoice, PaymentMethod.Cash, User.Identity.GetUserName());
            db.SaveChanges();

            new EmailHelper().SendInvoicePaymentConfirmation(invoice, true);

            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for invoice " + invoiceId + " manually via MarkBidderPaidManually");
            
            return RedirectToAction("Index", "Invoices");
        }

        public ActionResult RedirectToPayPal(int iid, string email)
        {
            var invoice = db.Invoices.FirstOrDefault(i => i.InvoiceId == iid && i.Email != null && i.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (invoice == null)
            {
                return HttpNotFound();
            }

            var model = new InvoiceForPayPal(invoice);

            return View(model);
        }

        [HttpPost]
        public ActionResult PayPalComplete(FormCollection form)
        {
            var ppTrans = new PayPalTransaction(form);
            db.PayPalTransactions.Add(ppTrans);
            db.SaveChanges(); //go ahead and record the transaction

            var invoiceId = InvoiceRepository.GetInvoiceIdFromTransaction(ppTrans);
            if (!invoiceId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            new InvoiceRepository(db).ApplyPaymentToInvoice(ppTrans, invoice);
            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for invoice  " + invoice.InvoiceId + " via cart post-back");

            if (invoice.Bids.Any() && invoice.Bidder != null)
            {
                //go to auction review page if there were auction winnings in this order
                return RedirectToAction("ReviewBidderWinnings", new { bid = invoice.Bidder.BidderId, email = invoice.Bidder.Email, selfPaySuccessful = true });
            }

            return View(invoice);
        }

        // GET: Invoices/Delete/5
        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);

            if (invoice == null)
            {
                return HttpNotFound();
            }

            new InvoiceRepository(db).DeleteInvoice(invoice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
