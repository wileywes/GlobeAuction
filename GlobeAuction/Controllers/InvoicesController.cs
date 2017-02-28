using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Models;
using GlobeAuction.Helpers;

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
                    b.ZipCode == invoiceLookupModel.ZipCode);

                if (bidder == null)
                {
                    ModelState.AddModelError("bidderId", "No bidder was found matching this information.");
                }
                else
                {
                    var winnings = db.AuctionItems.Where(ai => ai.WinningBidderId == invoiceLookupModel.BidderId).ToList();

                    if (winnings.Any())
                    {
                        var invoice = new InvoiceRepository(db).CreateInvoice(bidder, winnings);

                        return RedirectToAction("Review", new { bid = bidder.BidderId, iid = invoice.InvoiceId });
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
        public ActionResult Review(int bid, int iid)
        {
            //check they are
            var bidder = db.Bidders.FirstOrDefault(b => b.BidderId == bid);
            if (bidder == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var invoice = db.Invoices.FirstOrDefault(i => i.InvoiceId == iid);
            if (invoice == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            db.Entry(invoice).Reference(i => i.Bidder).Load();
            if (invoice.Bidder.BidderId != bid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            db.Entry(invoice).Collection(i => i.AuctionItems).Load();

            return View(invoice);
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
