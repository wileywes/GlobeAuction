using AutoMapper;
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

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanEditTickets)]
    public class StoreItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: StoreItems
        public ActionResult Index()
        {
            var models = db.StoreItems.ToList().Select(i => Mapper.Map<StoreItemViewModel>(i))
                .ToList() ?? new List<StoreItemViewModel>();
            return View(models);
        }

        // GET: StoreItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoreItem storeItem = db.StoreItems.Find(id);
            if (storeItem == null)
            {
                return HttpNotFound();
            }

            var model = Mapper.Map<StoreItemViewModel>(storeItem);
            return View(model);
        }

        // GET: StoreItems/Create
        public ActionResult Create()
        {
            return View(new StoreItemViewModel());
        }

        // POST: StoreItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "StoreItemId,CreateDate,UpdateDate,UpdateBy")] StoreItemViewModel storeItemViewModel)
        {
            if (ModelState.IsValid)
            {
                var storeItem = Mapper.Map<StoreItem>(storeItemViewModel);
                storeItem.CreateDate = storeItem.UpdateDate = DateTime.Now;
                storeItem.UpdateBy = User.Identity.GetUserName();

                db.StoreItems.Add(storeItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(storeItemViewModel);
        }

        // GET: StoreItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoreItem storeItem = db.StoreItems.Find(id);
            if (storeItem == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<StoreItemViewModel>(storeItem);
            return View(model);
        }

        // POST: StoreItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "UpdateDate,UpdateBy")] StoreItemViewModel storeItemViewModel)
        {
            if (ModelState.IsValid)
            {
                var storeItem = Mapper.Map<StoreItem>(storeItemViewModel);
                storeItem.UpdateDate = DateTime.Now;
                storeItem.UpdateBy = User.Identity.GetUserName();

                db.Entry(storeItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(storeItemViewModel);
        }

        // GET: StoreItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoreItem storeItem = db.StoreItems.Find(id);
            if (storeItem == null)
            {
                return HttpNotFound();
            }
            var model = Mapper.Map<StoreItemViewModel>(storeItem);
            return View(model);
        }

        // POST: StoreItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StoreItem storeItem = db.StoreItems.Find(id);
            db.StoreItems.Remove(storeItem);
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
