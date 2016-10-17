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
    public class DonationItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DonationItems
        [Authorize(Roles = Roles.CanEditItems)]
        public ActionResult Index()
        {
            return View(db.DonationItems.Where(i => i.IsDeleted == false).ToList());
        }

        // GET: DonationItems/Details/5
        [Authorize(Roles = Roles.CanEditItems)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonationItem donationItem = db.DonationItems.Find(id);
            if (donationItem == null)
            {
                return HttpNotFound();
            }
            return View(donationItem);
        }

        // GET: DonationItems/Create
        [AllowAnonymous]
        public ActionResult Create()
        {
            var previousSolicitor = TempData["PreviousSolicitor"] as Solicitor;
            if (previousSolicitor != null)
            {
                var anotherDonation = new DonationItem
                {
                    Solicitor = new Solicitor
                    {
                        Email = previousSolicitor.Email,
                        FirstName = previousSolicitor.FirstName,
                        LastName = previousSolicitor.LastName,
                        Phone = previousSolicitor.Phone
                    }
                };
                ViewData["showSuccess"] = true;
                return View(anotherDonation);
            }
            return View();
        }

        // POST: DonationItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Create([Bind(Exclude = "DonationItemId,CreateDate,UpdateDate,SolicitorId,DonorId")] DonationItem donationItem)
        {
            if (ModelState.IsValid)
            {
                donationItem.CreateDate = donationItem.UpdateDate = DateTime.Now;

                //tie to existing Solicitor by email
                var existingSolicitor = db.Solicitors.FirstOrDefault(s => s.Email.Equals(donationItem.Solicitor.Email, StringComparison.OrdinalIgnoreCase));
                if (existingSolicitor != null)
                    donationItem.Solicitor = existingSolicitor;

                db.DonationItems.Add(donationItem);
                db.SaveChanges();

                TempData["PreviousSolicitor"] = donationItem.Solicitor;
                return RedirectToAction("Create");
            }

            return View(donationItem);
        }

        // GET: DonationItems/Edit/5
        [Authorize(Roles = Roles.CanEditItems)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonationItem donationItem = db.DonationItems.Find(id);
            if (donationItem == null)
            {
                return HttpNotFound();
            }
            return View(donationItem);
        }

        // POST: DonationItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.CanEditItems)]
        public ActionResult Edit([Bind(Include = "DonationItemId,Category,Description,Restrictions,ExpirationDate,DollarValue,CreateDate,UpdateDate")] DonationItem donationItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(donationItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(donationItem);
        }

        // GET: DonationItems/Delete/5
        [Authorize(Roles = Roles.CanEditItems)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonationItem donationItem = db.DonationItems.Find(id);
            if (donationItem == null)
            {
                return HttpNotFound();
            }
            return View(donationItem);
        }

        // POST: DonationItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.CanEditItems)]
        public ActionResult DeleteConfirmed(int id)
        {
            DonationItem donationItem = db.DonationItems.Find(id);
            donationItem.IsDeleted = true;
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
