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
using Microsoft.AspNet.Identity;

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
            var donationItemsNotInAuctionItem = db.DonationItems.Where(di => !di.IsDeleted && !donationItemIdsInAuctionItem.Contains(di.DonationItemId)).ToList();

            foreach (var ai in auctionItems)
            {
                db.Entry(ai).Collection(a => a.DonationItems).Load();
            }

            var model = new ItemsViewModel
            {
                AuctionItems = auctionItems.Select(i => new AuctionItemViewModel(i)).ToList(),
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
            db.Entry(auctionItem).Collection(d => d.DonationItems).Load();
            return View(auctionItem);
        }

        // GET: AuctionItems/PrintBidSheets?auctionItemIds=1,2,3
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult PrintBidSheets(string auctionItemIds)
        {
            if (string.IsNullOrEmpty(auctionItemIds))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ids = auctionItemIds.Split(new[] { ',' }).Select(id => int.Parse(id)).ToList();

            var auctionItems = db.AuctionItems.Where(ai => ids.Contains(ai.AuctionItemId)).ToList();

            foreach (var ai in auctionItems)
            {
                db.Entry(ai).Collection(a => a.DonationItems).Load();
            }

            return View(auctionItems);
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
            db.Entry(auctionItem).Collection(d => d.DonationItems).Load();
            AddAuctionItemControlInfo(auctionItem);
            return View(auctionItem);
        }

        // POST: AuctionItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Edit([Bind(Include = "AuctionItemId,UniqueItemNumber,Description,Category,StartingBid,BidIncrement,CreateDate,WinningBidderId,WinningBid")] AuctionItem auctionItem)
        {
            if (ModelState.IsValid)
            {
                auctionItem.UpdateDate = DateTime.Now;
                auctionItem.UpdateBy = User.Identity.GetUserName();

                db.Entry(auctionItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            AddAuctionItemControlInfo(auctionItem);
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
            db.Entry(auctionItem).Collection(d => d.DonationItems).Load();
            return View(auctionItem);
        }

        // POST: AuctionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult DeleteConfirmed(int id)
        {
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            db.Entry(auctionItem).Collection(d => d.DonationItems).Load();
            auctionItem.DonationItems.Clear();            

            db.AuctionItems.Remove(auctionItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // POST: AuctionItems/Delete/5
        [HttpPost, ActionName("SubmitSelectedDonationItems")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult SubmitSelectedDonationItems(ItemsViewModel postedModel)
        {
            var selectedDonationIds = postedModel.DonationsNotInAuctionItem.Where(i => i.IsSelected)
                .Select(d => d.DonationItemId)
                .ToList();

            if (!selectedDonationIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var selectedDonations = selectedDonationIds.Select(id => db.DonationItems.Find(id)).ToList();
            var nextUniqueId = 1;

            if (db.AuctionItems.Any())
            {
                nextUniqueId = db.AuctionItems.Max(a => a.UniqueItemNumber) + 1;
            }

            var username = User.Identity.GetUserName();
            switch (postedModel.PostActionName)
            {
                case "MakeBasket":
                    var basket = ItemsHelper.CreateAuctionItemForDonations(nextUniqueId, selectedDonations, username);
                    db.AuctionItems.Add(basket);
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { id = basket.AuctionItemId });

                case "MakeSingle":
                    foreach(var selectedDonation in selectedDonations)
                    {
                        var auctionItem = ItemsHelper.CreateAuctionItemForDonation(nextUniqueId, selectedDonation, username);
                        db.AuctionItems.Add(auctionItem);
                        nextUniqueId++;
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index");
            }

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

        private void AddAuctionItemControlInfo(AuctionItem item)
        {
            var auctionItemCategories = AuctionConstants.DonationItemCategories.Select(c => new SelectListItem { Text = c, Value = c }).ToList();

            //additional categories for auction items
            auctionItemCategories.Add(new SelectListItem { Text = "Live", Value = "Live" });
            auctionItemCategories.Add(new SelectListItem { Text = "Teacher Treasures", Value = "Teacher Treasures" });

            if (item != null && !string.IsNullOrEmpty(item.Category))
            {
                var selected = auctionItemCategories.FirstOrDefault(c => c.Value.Equals(item.Category));
                if (selected != null) selected.Selected = true;
            }


            ViewBag.AuctionItemCategories = auctionItemCategories;
        }
    }
}
