using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Helpers;
using GlobeAuction.Models;

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanAdminUsers)]
    public class AuctionCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AuctionCategories
        public ActionResult Index()
        {
            return View(db.AuctionCategories.ToList());
        }


        [HttpPost, ActionName("SubmitCategoriesAction")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult SubmitCategoriesAction(string categoriesAction)
        {
            var currentEst = Utilities.GetEasternTimeNow();
            switch (categoriesAction)
            {
                case "OpenAllNow":
                    foreach(var cat in db.AuctionCategories)
                    {
                        cat.BidOpenDateLtz = currentEst.AddDays(-30);
                        cat.BidCloseDateLtz = currentEst.AddDays(30);
                    }
                    db.SaveChanges();
                    new ItemsRepository(db).ClearCatalogDataCache();
                    break;
                case "CloseAllNow":
                    foreach (var cat in db.AuctionCategories)
                    {
                        cat.BidOpenDateLtz = currentEst.AddDays(120);
                        cat.BidCloseDateLtz = currentEst.AddDays(120);
                    }
                    db.SaveChanges();
                    new ItemsRepository(db).ClearCatalogDataCache();
                    break;
                default:
                    throw new ApplicationException("Unrecognized action");
            }

            return RedirectToAction("Index");
        }


        // GET: AuctionCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionCategory auctionCategory = db.AuctionCategories.Find(id);
            if (auctionCategory == null)
            {
                return HttpNotFound();
            }
            return View(auctionCategory);
        }

        // GET: AuctionCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AuctionCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AuctionCategoryId,Name,ItemNumberStart,ItemNumberEnd,BidOpenDateLtz,BidCloseDateLtz,IsFundAProject,IsOnlyAvailableToAuctionItems")] AuctionCategory auctionCategory)
        {
            if (ModelState.IsValid)
            {
                db.AuctionCategories.Add(auctionCategory);
                db.SaveChanges();
                new ItemsRepository(db).ClearCatalogDataCache();
                return RedirectToAction("Index");
            }

            return View(auctionCategory);
        }

        // GET: AuctionCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionCategory auctionCategory = db.AuctionCategories.Find(id);
            if (auctionCategory == null)
            {
                return HttpNotFound();
            }
            return View(auctionCategory);
        }

        // POST: AuctionCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AuctionCategoryId,Name,ItemNumberStart,ItemNumberEnd,BidOpenDateLtz,BidCloseDateLtz,IsFundAProject,IsOnlyAvailableToAuctionItems")] AuctionCategory auctionCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(auctionCategory).State = EntityState.Modified;
                db.SaveChanges();
                new ItemsRepository(db).ClearCatalogDataCache();
                return RedirectToAction("Index");
            }
            return View(auctionCategory);
        }

        // GET: AuctionCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionCategory auctionCategory = db.AuctionCategories.Find(id);
            if (auctionCategory == null)
            {
                return HttpNotFound();
            }
            return View(auctionCategory);
        }

        // POST: AuctionCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AuctionCategory auctionCategory = db.AuctionCategories.Find(id);
            if (auctionCategory == null)
            {
                return HttpNotFound();
            }

            var auctionItemCount = db.AuctionItems.Count(i => i.Category.AuctionCategoryId == id);

            if (auctionItemCount > 0)
            {
                ModelState.AddModelError("", $"There are still {auctionItemCount} auction items tied to this category.  You must move these items to a different category first.");
            }
            else
            {
                db.AuctionCategories.Remove(auctionCategory);
                db.SaveChanges();
                new ItemsRepository(db).ClearCatalogDataCache();
                return RedirectToAction("Index");
            }
            return View(auctionCategory);
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
