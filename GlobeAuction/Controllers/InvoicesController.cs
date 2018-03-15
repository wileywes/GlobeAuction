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
    public class InvoicesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invoices
        [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
        public ActionResult Index()
        {
            var invoices = db.Invoices
                .Include(i => i.AuctionItems)
                .Include(i => i.Bidder)
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
        
        [AllowAnonymous]
        public ActionResult Checkout()
        {
            return View(new InvoiceLookupModel());
        }

        // POST: AuctionItems/Delete/5
        [HttpPost, ActionName("Checkout")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult CheckoutConfirmed(InvoiceLookupModel invoiceLookupModel)
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
                    b.BidderNumber == invoiceLookupModel.BidderNumber &&
                    b.LastName.Equals(invoiceLookupModel.LastName, StringComparison.OrdinalIgnoreCase) &&
                    (!requireEmailMatch || b.Email.Equals(invoiceLookupModel.Email, StringComparison.OrdinalIgnoreCase)));

                if (bidder == null)
                {
                    ModelState.AddModelError("bidderId", "No bidder was found matching this information.");
                }
                else
                {
                    //send to review page once we've verified who they are
                    return RedirectToAction("ReviewBidderWinnings", new { bid = bidder.BidderId, email = bidder.Email });
                }
            }

            return View(invoiceLookupModel);
        }

        [AllowAnonymous]
        public ActionResult ReviewBidderWinnings(int bid, string email, bool? manualPaidSuccessful, bool? selfPaySuccessful)
        {
            //check they are who they say they are
            var bidder = db.Bidders.FirstOrDefault(b => b.IsDeleted == false && b.BidderId == bid && b.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (bidder == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var invoicesForBidder = db.Invoices
                .Include(i => i.AuctionItems)
                .Include(i => i.PaymentTransaction)
                .Include(i => i.StoreItemPurchases)
                .Where(i => i.Bidder.BidderId == bidder.BidderId)
                .ToList();

            var auctionWinningsForBidderNotInInvoice = db.AuctionItems
                .Include(a => a.DonationItems)
                .Where(a => a.WinningBidderId.HasValue && a.WinningBidderId.Value == bidder.BidderId && a.Invoice == null)
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
        [AllowAnonymous]
        public ActionResult ReviewBidderWinningsPayNow(ReviewBidderWinningsViewModel model, string submitButton)
        {
            var bidder = db.Bidders.FirstOrDefault(b => b.IsDeleted == false && b.BidderId == model.BidderId && b.Email.Equals(model.BidderEmail, StringComparison.OrdinalIgnoreCase));
            if (bidder == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var auctionItemIds = model.AuctionItemsNotInInvoice.Select(i => i.AuctionItemId).ToList();
            var winnings = db.AuctionItems.Where(ai => auctionItemIds.Contains(ai.AuctionItemId) &&
                    ai.WinningBidderId.HasValue && 
                    ai.WinningBidderId.Value == bidder.BidderId && 
                    ai.Invoice == null).ToList();

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

        [AllowAnonymous]
        public ActionResult RemoveAuctionItemFromUnpaidInvoice(int invoiceId, int auctionItemId)
        {
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null) return HttpNotFound();
            if (invoice.IsPaid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //grab a copy before we delete the invoice
            var bidId = invoice.Bidder.BidderId;
            var bidEmail = invoice.Bidder.Email;

            var item = db.AuctionItems.FirstOrDefault(ai => ai.Invoice != null && ai.Invoice.InvoiceId == invoiceId && ai.AuctionItemId == auctionItemId);
            if (item == null)
            {
                return HttpNotFound();
            }

            item.Invoice = null;
            invoice.AuctionItems.Remove(invoice.AuctionItems.First(ai => ai.AuctionItemId == auctionItemId));

            //delete the invoice entirely if there are no more lines on it
            if (invoice.AuctionItems.Count == 0 && invoice.StoreItemPurchases.Count == 0)
            {
                db.Invoices.Remove(invoice);
            }
            db.SaveChanges();

            return RedirectToAction("ReviewBidderWinnings", new { bid = bidId, email = bidEmail });
        }

        [AllowAnonymous]
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
            if (invoice.AuctionItems.Count == 0 && invoice.StoreItemPurchases.Count == 0)
            {
                db.Invoices.Remove(invoice);
            }

            db.SaveChanges();

            return RedirectToAction("ReviewBidderWinnings", new { bid = bidId, email = bidEmail });
        }

        [AllowAnonymous]
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
        
        [AllowAnonymous]
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

            if (invoice.AuctionItems.Any() && invoice.Bidder != null)
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

            foreach(var sip in db.StoreItemPurchases.Where(p => p.Invoice.InvoiceId == id).ToList())
            {
                //add the quantity back if it's not a raffle ticket
                if (sip.StoreItem.IsRaffleTicket == false)
                {
                    sip.StoreItem.Quantity += sip.Quantity;
                }

                //remove store item purchase
                db.StoreItemPurchases.Remove(sip);
            }

            foreach (var ai in db.AuctionItems.Where(p => p.Invoice.InvoiceId == id))
            {
                //detach auction items
                ai.Invoice = null;
            }

            db.Invoices.Remove(invoice);
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
