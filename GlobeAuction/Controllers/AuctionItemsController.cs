using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Models;

namespace GlobeAuction.Controllers
{
    public class AuctionItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AuctionItems
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Index()
        {
            var auctionItems = db.AuctionItems.ToList();
            var donationItemIdsInAuctionItem = db.AuctionItems.SelectMany(ai => ai.DonationItems.Select(di => di.DonationItemId)).ToList();
            var donationItemsNotInAuctionItem = db.DonationItems.Where(di => !donationItemIdsInAuctionItem.Contains(di.DonationItemId)).ToList();

            var model = new ItemsViewModel
            {
                AuctionItems = auctionItems,
                DonationsNotInAuctionItem = donationItemsNotInAuctionItem.Select(d => new DonationItemViewModel(d)).ToList()
            };
            return View(model);
        }

        // GET: AuctionItems/Details/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            if (auctionItem == null)
            {
                return HttpNotFound();
            }
            return View(auctionItem);
        }

        // GET: AuctionItems/Create
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: AuctionItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Create([Bind(Include = "AuctionItemId,UniqueItemNumber,Description,Category,StartingBid,BidIncrement,CreateDate,UpdateDate,UpdateBy,WinningBidderId,WinningBid")] AuctionItem auctionItem)
        {
            if (ModelState.IsValid)
            {
                db.AuctionItems.Add(auctionItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(auctionItem);
        }

        // GET: AuctionItems/Edit/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            if (auctionItem == null)
            {
                return HttpNotFound();
            }
            return View(auctionItem);
        }

        // POST: AuctionItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Edit([Bind(Include = "AuctionItemId,UniqueItemNumber,Description,Category,StartingBid,BidIncrement,CreateDate,UpdateDate,UpdateBy,WinningBidderId,WinningBid")] AuctionItem auctionItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(auctionItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(auctionItem);
        }

        // GET: AuctionItems/Delete/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            if (auctionItem == null)
            {
                return HttpNotFound();
            }
            return View(auctionItem);
        }

        // POST: AuctionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult DeleteConfirmed(int id)
        {
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            db.AuctionItems.Remove(auctionItem);
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
