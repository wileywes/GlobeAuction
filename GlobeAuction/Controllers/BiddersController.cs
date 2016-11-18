using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Models;
using Microsoft.AspNet.Identity;
using System.Configuration;
using GlobeAuction.Helpers;

namespace GlobeAuction.Controllers
{
    public class BiddersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bidders
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Index()
        {
            var bidders = db.Bidders.ToList();

            foreach (var bidder in bidders)
            {
                db.Entry(bidder).Collection(a => a.AuctionGuests).Load();
            }
            var models = bidders.Select(b => new BidderForList(b)).ToList();
            
            return View(models);
        }

        // GET: Bidders/Details/5
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            db.Entry(bidder).Collection(b => b.Students).Load();
            db.Entry(bidder).Collection(b => b.AuctionGuests).Load();
            return View(bidder);
        }

        // GET: Bidders/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var bidder = new Bidder
            {
                AuctionGuests = new List<AuctionGuest>(Enumerable.Repeat(new AuctionGuest(), 6)),
                Students = new List<Student>(Enumerable.Repeat(new Student(), 4)),
            };
            AddBidderControlInfo();
            return View(bidder);
        }

        // POST: Bidders/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "BidderId")] Bidder bidder, string submitButton)
        {
            if (ModelState.IsValid)
            {
                bidder.CreateDate = bidder.UpdateDate = DateTime.Now;
                bidder.UpdateBy = bidder.Email;

                //strip out dependents that weren't filled in
                bidder.Students = bidder.Students.Where(s => !string.IsNullOrEmpty(s.HomeroomTeacher)).ToList();
                bidder.AuctionGuests = bidder.AuctionGuests.Where(g => !string.IsNullOrEmpty(g.FirstName)).ToList();

                foreach(var guest in bidder.AuctionGuests)
                {
                    var ticketType = db.TicketTypes.Find(int.Parse(guest.TicketType));
                    guest.TicketType = ticketType.Name;
                    guest.TicketPrice = ticketType.Price;
                }
                
                db.Bidders.Add(bidder);
                db.SaveChanges();

                if (submitButton == "Create Bidder Only")
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("RedirectToPayPal", new { id = bidder.BidderId });
            }

            AddBidderControlInfo();
            return View(bidder);
        }
        
        [AllowAnonymous]
        public ActionResult RedirectToPayPal(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            db.Entry(bidder).Collection(b => b.AuctionGuests).Load();
            var viewModel = new BidderForPayPal(bidder);
            ViewBag.PayPalBusiness = ConfigurationManager.AppSettings["PayPalBusiness"];

            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult PayPalComplete(FormCollection form)
        {
            var ppTrans = new PayPalTransaction(form);
            db.PayPalTransactions.Add(ppTrans);
            db.SaveChanges(); //go ahead and record the transaction

            var bidderIdStr = form["custom"];
            if (string.IsNullOrEmpty(bidderIdStr))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var id = int.Parse(bidderIdStr);
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }
            
            db.Entry(bidder).Collection(b => b.AuctionGuests).Load();

            if (ppTrans.WasPaymentSuccessful)
            {
                var paymentLeft = ppTrans.PaymentGross;

                foreach (var guest in bidder.AuctionGuests)
                {
                    var priceToUseUp = Math.Min(guest.TicketPrice, paymentLeft);
                    guest.TicketPricePaid = priceToUseUp;
                    guest.TicketTransaction = ppTrans;
                    paymentLeft -= priceToUseUp;
                }
                db.SaveChanges();

                new EmailHelper().SendBidderPaymentConfirmation(bidder, ppTrans);
            }

            return View(bidder);
        }

        // GET: Bidders/Edit/5
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            db.Entry(bidder).Collection(b => b.Students).Load();
            db.Entry(bidder).Collection(b => b.AuctionGuests).Load();
            return View(bidder);
        }

        // POST: Bidders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "UpdateBy,UpdateDate")] Bidder bidder)
        {
            if (ModelState.IsValid)
            {
                bidder.UpdateBy = User.Identity.GetUserName();
                bidder.UpdateDate = DateTime.Now;

                foreach(var g in bidder.AuctionGuests)
                {
                    db.Entry(g).State = EntityState.Modified;
                }
                foreach (var s in bidder.Students)
                {
                    db.Entry(s).State = EntityState.Modified;
                }

                db.Entry(bidder).State = EntityState.Modified;
                
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bidder);
        }

        // GET: Bidders/Delete/5
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }
            db.Entry(bidder).Collection(b => b.Students).Load();
            db.Entry(bidder).Collection(b => b.AuctionGuests).Load();
            return View(bidder);
        }

        // POST: Bidders/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Bidder bidder = db.Bidders.Find(id);

            db.Entry(bidder).Collection(b => b.Students).Load();
            db.Entry(bidder).Collection(b => b.AuctionGuests).Load();

            db.AuctionGuests.RemoveRange(bidder.AuctionGuests);
            db.Students.RemoveRange(bidder.Students);
            db.Bidders.Remove(bidder);

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

        private void AddBidderControlInfo()
        {
            var ticketTypes = db.TicketTypes.ToList();

            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanEditBidders))
            {
                //remove admin-only ticket types
                ticketTypes = ticketTypes.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }

            ViewBag.TicketTypes = ticketTypes
                .Select(t => new SelectListItem { Text = string.Format("{0} - {1:C}", t.Name, t.Price), Value = t.TicketTypeId.ToString() }).ToList();

            ViewBag.TeacherNames = AuctionConstants.TeacherNames
                .Select(t => new SelectListItem { Text = t, Value = t });
        }
    }
}
