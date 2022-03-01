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
using Microsoft.AspNet.Identity;

namespace GlobeAuction.Controllers
{
    public class FaqCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FaqCategories
        public ActionResult Index()
        {
            return View(db.FaqCategories.ToList());
        }

        // GET: FaqCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FaqCategory faqCategory = db.FaqCategories.Find(id);
            if (faqCategory == null)
            {
                return HttpNotFound();
            }
            return View(faqCategory);
        }

        // GET: FaqCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FaqCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FaqCategoryId,Name,DisplayOrder")] FaqCategory faqCategory)
        {
            if (ModelState.IsValid)
            {
                db.FaqCategories.Add(faqCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(faqCategory);
        }

        // GET: FaqCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FaqCategory faqCategory = db.FaqCategories.Find(id);
            if (faqCategory == null)
            {
                return HttpNotFound();
            }
            return View(faqCategory);
        }

        // POST: FaqCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FaqCategoryId,Name,DisplayOrder")] FaqCategory faqCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(faqCategory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(faqCategory);
        }

        // GET: FaqCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FaqCategory faqCategory = db.FaqCategories.Find(id);
            if (faqCategory == null)
            {
                return HttpNotFound();
            }
            return View(faqCategory);
        }

        // POST: FaqCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FaqCategory faqCategory = db.FaqCategories.Find(id);
            db.FaqCategories.Remove(faqCategory);
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
