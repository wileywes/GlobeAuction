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
    public class FaqsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Faqs
        public ActionResult List()
        {
            return View(db.Faqs.Include(f => f.Category).ToList());
        }

        // GET: Faqs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faq faq = db.Faqs.Find(id);
            if (faq == null)
            {
                return HttpNotFound();
            }
            return View(faq);
        }

        // GET: Faqs/Create
        public ActionResult Create()
        {
            AddCategoryControlInfo(null);
            return View();
        }

        // POST: Faqs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Question,Answer,OrderInCategory")] Faq faq, string categorySelect)
        {
            //do category with special handling
            ModelState.Remove("Category");

            if (!string.IsNullOrEmpty(categorySelect))
            {
                if (ModelState.IsValid)
                {
                    faq.Category = db.FaqCategories.Find(int.Parse(categorySelect));
                    faq.CreateDate = faq.UpdateDate = Utilities.GetEasternTimeNow();
                    faq.UpdateBy = User.Identity.GetUserName();

                    db.Faqs.Add(faq);
                    db.SaveChanges();
                    return RedirectToAction("List");
                }
            }
            else
            {
                ModelState.AddModelError("category", "You must select a category.");
            }

            AddCategoryControlInfo(faq);
            return View(faq);
        }

        // GET: Faqs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faq faq = db.Faqs.Find(id);
            if (faq == null)
            {
                return HttpNotFound();
            }

            AddCategoryControlInfo(faq);
            return View(faq);
        }

        // POST: Faqs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FaqId,Question,Answer,OrderInCategory,CreateDate")] Faq faq, string categorySelect)
        {
            //do category with special handling
            ModelState.Remove("Category");

            if (!string.IsNullOrEmpty(categorySelect))
            {
                if (ModelState.IsValid)
                {
                    db.Faqs.Attach(faq);
                    db.Entry(faq).Reference("Category").Load();
                    faq.Category = db.FaqCategories.Find(int.Parse(categorySelect));

                    faq.UpdateDate = Utilities.GetEasternTimeNow();
                    faq.UpdateBy = User.Identity.GetUserName();

                    db.Entry(faq).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("List");
                }
            }
            else
            {
                ModelState.AddModelError("category", "You must select a category.");
            }

            AddCategoryControlInfo(faq);
            return View(faq);
        }

        // GET: Faqs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faq faq = db.Faqs.Find(id);
            if (faq == null)
            {
                return HttpNotFound();
            }
            return View(faq);
        }

        // POST: Faqs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Faq faq = db.Faqs.Find(id);
            db.Faqs.Remove(faq);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddCategoryControlInfo(Faq faq)
        {
            var categories = db.FaqCategories.ToList();
            var faqCategories = categories.Select(c => new SelectListItem { Text = c.Name, Value = c.FaqCategoryId.ToString() }).ToList();

            if (faq != null && faq.Category != null)
            {
                var selected = faqCategories.FirstOrDefault(c => c.Value.Equals(faq.Category.FaqCategoryId.ToString()));
                if (selected != null) selected.Selected = true;
            }

            ViewBag.FaqCategories = faqCategories;
        }
    }
}
