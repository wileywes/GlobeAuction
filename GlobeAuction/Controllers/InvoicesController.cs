﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Models;
using GlobeAuction.Helpers;
using System.Configuration;

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
    public class InvoicesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invoices
        public ActionResult Index()
        {
            return View(db.Invoices.ToList());
        }

        // GET: Invoices/Details/5
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
            if (ModelState.IsValid)
            {
                var bidder = db.Bidders.FirstOrDefault(b =>
                    b.BidderId == invoiceLookupModel.BidderId &&
                    b.LastName.Equals(invoiceLookupModel.LastName, StringComparison.OrdinalIgnoreCase) &&
                    b.Email.Equals(invoiceLookupModel.Email, StringComparison.OrdinalIgnoreCase));

                if (bidder == null)
                {
                    ModelState.AddModelError("bidderId", "No bidder was found matching this information.");
                }
                else
                {
                    //TODO: only pull items not on an invoice already
                    var winnings = db.AuctionItems.Where(ai => ai.WinningBidderId == invoiceLookupModel.BidderId).ToList();

                    if (winnings.Any())
                    {
                        var invoice = new InvoiceRepository(db).CreateInvoice(bidder, winnings);

                        return RedirectToAction("Review", new { bid = bidder.BidderId, email = bidder.Email });
                    }
                    else
                    {
                        ModelState.AddModelError("bidderId", "At this time there are no auction items recorded as being won by you.");
                    }
                }
            }

            return View(invoiceLookupModel);
        }

        [AllowAnonymous]
        public ActionResult Review(int bid, string email)
        {
            //check they are
            var bidder = db.Bidders.FirstOrDefault(b => b.BidderId == bid && b.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (bidder == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var invoicesForBidder = db.Invoices.Where(i => i.Bidder.BidderId == bidder.BidderId);
            
            foreach(var invoice in invoicesForBidder)
            {
                db.Entry(invoice).Collection(i => i.AuctionItems).Load();
                db.Entry(invoice).Reference(i => i.PaymentTransaction).Load();
                db.Entry(invoice).Reference(i => i.Bidder).Load();
            }

            var viewModel = new InvoicesForBidderViewModel(bidder, invoicesForBidder);
            
            return View(viewModel);
        }


        [AllowAnonymous]
        public ActionResult RedirectToPayPal(int bid, string email, int iid)
        {
            var bidder = db.Bidders.FirstOrDefault(b => b.BidderId == bid && b.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (bidder == null)
            {
                return HttpNotFound();
            }

            var invoice = db.Invoices.FirstOrDefault(i => i.InvoiceId == iid && i.Bidder.BidderId == bidder.BidderId);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            db.Entry(invoice).Reference(i => i.Bidder).Load();
            db.Entry(invoice).Reference(i => i.AuctionItems).Load();

            ViewBag.PayPalBusiness = ConfigurationManager.AppSettings["PayPalBusiness"];

            return View(invoice);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult PayPalComplete(FormCollection form)
        {
            var ppTrans = new PayPalTransaction(form);
            db.PayPalTransactions.Add(ppTrans);
            db.SaveChanges(); //go ahead and record the transaction

            var bidderId = BidderRepository.GetBidderIdFromTransaction(ppTrans);
            if (!bidderId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Bidder bidder = db.Bidders.Find(bidderId.Value);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            new BidderRepository(db).ApplyTicketPaymentToBidder(ppTrans, bidder);
            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for bidder " + bidder.BidderId + " via cart post-back");

            return View(bidder);
        }



        // GET: Invoices/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: Invoices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InvoiceId,IsPaid,WasMarkedPaidManually,CreateDate,UpdateDate,UpdateBy")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(invoice);
        }

        // GET: Invoices/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);
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
