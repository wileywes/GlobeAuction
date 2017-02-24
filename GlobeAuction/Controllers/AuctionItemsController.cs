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
    [Authorize(Roles = AuctionRoles.CanEditItems)]
    public class AuctionItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AuctionItems
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
        public ActionResult PrintBidSheets(string auctionItemIds)
        {
            if (string.IsNullOrEmpty(auctionItemIds))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ids = auctionItemIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)).ToList();

            var auctionItems = db.AuctionItems.Where(ai => ids.Contains(ai.UniqueItemNumber)).ToList();

            foreach (var ai in auctionItems)
            {
                db.Entry(ai).Collection(a => a.DonationItems).Load();
            }

            return View(auctionItems);
        }
        
        // GET: AuctionItems/Edit/5
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
        public ActionResult Edit([Bind(Include = "AuctionItemId,UniqueItemNumber,Title,Description,Category,StartingBid,BidIncrement,CreateDate,WinningBidderId,WinningBid")] AuctionItem auctionItem)
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

        // POST: AuctionItems/RemoveFromBasket/5
        [HttpGet]
        public ActionResult RemoveFromBasket(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.FirstOrDefault(ai => ai.DonationItems.Any(di => di.DonationItemId == id));
            if (auctionItem == null)
            {
                return HttpNotFound();
            }

            db.Entry(auctionItem).Collection(d => d.DonationItems).Load();
            var donationItem = auctionItem.DonationItems.First(di => di.DonationItemId == id);
            auctionItem.DonationItems.Remove(donationItem);
            auctionItem.UpdateDate = DateTime.Now;
            auctionItem.UpdateBy = User.Identity.GetUserName();

            db.Entry(auctionItem).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = auctionItem.AuctionItemId });
        }

        
        // GET: AuctionItems/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            db.Entry(auctionItem).Collection(d => d.DonationItems).Load();
            auctionItem.DonationItems.Clear();            

            db.AuctionItems.Remove(auctionItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        [HttpPost, ActionName("SubmitSelectedDonationItems")]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitSelectedDonationItems(string donationItemsAction, string selectedDonationItemIds, int? basketItemNumber)
        {
            var selectedDonationIds = selectedDonationItemIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse);

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
            switch (donationItemsAction)
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
                case "AddToBasket":
                    if (!basketItemNumber.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var existingAuctionItem = db.AuctionItems.FirstOrDefault(ai => ai.UniqueItemNumber == basketItemNumber.Value);
                    if (existingAuctionItem == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    db.Entry(existingAuctionItem).Collection(d => d.DonationItems).Load();
                    foreach (var selectedDonation in selectedDonations)
                    {
                        existingAuctionItem.DonationItems.Add(selectedDonation);
                    }
                    existingAuctionItem.UpdateDate = DateTime.Now;
                    existingAuctionItem.UpdateBy = username;
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { id = existingAuctionItem.AuctionItemId });
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("SubmitSelectedAuctionItems")]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitSelectedAuctionItems(string postActionName, string selectedAuctionItemIds, int? startingAuctionItemNumber)
        {
            var selectedAuctionIds = selectedAuctionItemIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            if (!selectedAuctionIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var selectedAuctionItems = db.AuctionItems.Where(ai => selectedAuctionIds.Contains(ai.UniqueItemNumber)).ToList();
            if (!selectedAuctionItems.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var username = User.Identity.GetUserName();
            switch (postActionName)
            {
                case "RenumberAuctionItems":
                    var nextItemNum = startingAuctionItemNumber.GetValueOrDefault(0);
                    if (nextItemNum <= 0)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    foreach (var auctionItem in selectedAuctionItems.OrderBy(a => a.UniqueItemNumber))
                    {
                        auctionItem.UniqueItemNumber = nextItemNum;
                        auctionItem.UpdateBy = username;
                        auctionItem.UpdateDate = DateTime.Now;
                        nextItemNum++;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
                
        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult Winners()
        {
            AddAuctionItemCategoryControlInfo(null);

            return View(new WinnersViewModel());
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult GetNextAuctionItemWithNoWinner(string selectedCategory, int currentAuctionItemId)
        {
            var nextItem = db.AuctionItems.FirstOrDefault(ai =>
                ai.WinningBidderId.HasValue == false &&
                ai.Category == selectedCategory &&
                ai.UniqueItemNumber > currentAuctionItemId);
            
            return Json(
                new { hasNext = nextItem != null, nextItem = nextItem },
                JsonRequestBehavior.AllowGet
            );
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
            AddAuctionItemCategoryControlInfo(item != null ? item.Category : null);
        }

        private void AddAuctionItemCategoryControlInfo(string selectedCategory)
        {
            var auctionItemCategories = AuctionConstants.DonationItemCategories.Select(c => new SelectListItem { Text = c, Value = c }).ToList();

            //additional categories for auction items
            auctionItemCategories.Add(new SelectListItem { Text = "Live", Value = "Live" });
            auctionItemCategories.Add(new SelectListItem { Text = "Teacher Treasures", Value = "Teacher Treasures" });

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                var selected = auctionItemCategories.FirstOrDefault(c => c.Value.Equals(selectedCategory));
                if (selected != null) selected.Selected = true;
            }


            ViewBag.AuctionItemCategories = auctionItemCategories;
        }
    }
}
