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
    public class DonationItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DonationItems
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Index()
        {
            return View(db.DonationItems.Where(i => i.IsDeleted == false).ToList());
        }

        // GET: DonationItems/Details/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
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
            db.Entry(donationItem).Reference(d => d.Donor).Load();
            db.Entry(donationItem).Reference(d => d.Solicitor).Load();
            return View(donationItem);
        }

        // GET: DonationItems/Create
        [AllowAnonymous]
        public ActionResult Create()
        {
            AddDonationItemControlInfo(null);
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
        public ActionResult Create([Bind(Exclude = "DonationItemId,CreateDate,UpdateDate,SolicitorId,DonorId")] DonationItem donationItem, string quantity)
        {
            int qty;
            if (int.TryParse(quantity, out qty) && qty >= 1 && qty <= 20)
            {
                if (ModelState.IsValid)
                {
                    //tie to existing Solicitor by email
                    var existingSolicitor = db.Solicitors.FirstOrDefault(s => s.Email.Equals(donationItem.Solicitor.Email, StringComparison.OrdinalIgnoreCase));
                    if (existingSolicitor != null)
                        donationItem.Solicitor = existingSolicitor;

                    donationItem.CreateDate = donationItem.UpdateDate = DateTime.Now;
                    donationItem.UpdateBy = donationItem.Solicitor.Email;

                    db.DonationItems.Add(donationItem);

                    if (qty > 1)
                    {
                        for(int i=1; i < qty; i++)
                        {
                            var newDonation = donationItem.Clone();
                            db.DonationItems.Add(newDonation);
                        }
                    }
                    db.SaveChanges();

                    TempData["PreviousSolicitor"] = donationItem.Solicitor;
                    return RedirectToAction("Create");
                }
            }
            else
            {
                ModelState.AddModelError("quantity", "Quantity must be a number between 1 and 20.");
            }

            AddDonationItemControlInfo(donationItem);
            return View(donationItem);
        }

        // GET: DonationItems/Edit/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
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
            db.Entry(donationItem).Reference(d => d.Donor).Load();
            db.Entry(donationItem).Reference(d => d.Solicitor).Load();
            AddDonationItemControlInfo(donationItem);
            return View(donationItem);
        }

        // POST: DonationItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Edit([Bind(Exclude = "UpdateDate")] DonationItem donationItem)
        {
            if (ModelState.IsValid)
            {
                donationItem.UpdateDate = DateTime.Now;
                donationItem.UpdateBy = User.Identity.GetUserName();

                db.Entry(donationItem).State = EntityState.Modified;

                db.Entry(donationItem.Donor).State = EntityState.Modified;
                db.Entry(donationItem.Solicitor).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index", "AuctionItems");
            }
            db.Entry(donationItem).Reference(d => d.Donor).Load();
            db.Entry(donationItem).Reference(d => d.Solicitor).Load();
            AddDonationItemControlInfo(donationItem);
            return View(donationItem);
        }

        // GET: DonationItems/Delete/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
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
            db.Entry(donationItem).Reference(d => d.Donor).Load();
            db.Entry(donationItem).Reference(d => d.Solicitor).Load();
            return View(donationItem);
        }

        // POST: DonationItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult DeleteConfirmed(int id)
        {
            DonationItem donationItem = db.DonationItems.Find(id);
            donationItem.IsDeleted = true;
            donationItem.UpdateBy = User.Identity.GetUserName();
            db.SaveChanges();
            return RedirectToAction("Index", "AuctionItems");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddDonationItemControlInfo(DonationItem donationItem)
        {
           var donationItemCategories = new List<SelectListItem>
           {
               new SelectListItem { Text="Restaurant Gift Card", Value="Restaurant Gift Card" },
               new SelectListItem { Text="Tickets, Memberships, Experiences & Getaways", Value="Tickets, Memberships, Experiences & Getaways" },
               new SelectListItem { Text="Health, Beauty and Fitness", Value="Health, Beauty and Fitness" },
               new SelectListItem { Text="Camps", Value="Camps" },
               new SelectListItem { Text="Services", Value="Services" }
           };

            if (donationItem != null && !string.IsNullOrEmpty(donationItem.Category))
            {
                var selected = donationItemCategories.FirstOrDefault(c => c.Value.Equals(donationItem.Category));
                if (selected != null) selected.Selected = true;
            }
            

            ViewBag.DonationItemCategories = donationItemCategories;
        }
    }
}
