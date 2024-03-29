﻿using System;
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
    [Authorize(Roles = AuctionRoles.CanEditItems)]
    public class DonationItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DonationItems
        public ActionResult Index()
        {
            var items = db.DonationItems
                .Include(d => d.Category)
                .Where(d => d.IsDeleted == false)
                .ToList();

            return View(items);
        }

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
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "DonationItemId,CreateDate,UpdateDate,SolicitorId,DonorId")] DonationItem donationItem, string quantity, string categorySelect)
        {
            //do category with special handling
            ModelState.Remove("Category");

            int qty;
            if (int.TryParse(quantity, out qty) && qty >= 1 && qty <= 20)
            {
                if (!string.IsNullOrEmpty(categorySelect))
                {                    
                    if (ModelState.IsValid)
                    {
                        //tie to existing Solicitor by email
                        var existingSolicitor = db.Solicitors.FirstOrDefault(s => s.Email.Equals(donationItem.Solicitor.Email, StringComparison.OrdinalIgnoreCase));
                        if (existingSolicitor != null)
                            donationItem.Solicitor = existingSolicitor;

                        donationItem.Category = db.AuctionCategories.Find(int.Parse(categorySelect));
                        donationItem.CreateDate = donationItem.UpdateDate = Utilities.GetEasternTimeNow();
                        donationItem.UpdateBy = donationItem.Solicitor.Email;

                        db.DonationItems.Add(donationItem);
                        db.SaveChanges();

                        TempData["PreviousSolicitor"] = donationItem.Solicitor;
                        return RedirectToAction("Create");
                    }
                }
                else
                {
                    ModelState.AddModelError("category", "You must select a category.");
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
            AddDonationItemControlInfo(donationItem);
            return View(donationItem);
        }

        // POST: DonationItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "UpdateDate")] DonationItem donationItem, string categorySelect)
        {
            //do category with special handling
            ModelState.Remove("Category");

            if (!string.IsNullOrEmpty(categorySelect))
            {
                if (ModelState.IsValid)
                {
                    db.DonationItems.Attach(donationItem);
                    db.Entry(donationItem).Reference("Category").Load();
                    donationItem.Category = db.AuctionCategories.Find(int.Parse(categorySelect));

                    donationItem.UpdateDate = Utilities.GetEasternTimeNow();
                    donationItem.UpdateBy = User.Identity.GetUserName();

                    db.Entry(donationItem).State = EntityState.Modified;

                    //donor and solicitor are editable on the page so mark as modified
                    db.Entry(donationItem.Donor).State = EntityState.Modified;
                    db.Entry(donationItem.Solicitor).State = EntityState.Modified;

                    db.SaveChanges();
                    return RedirectToAction("Index", "AuctionItems");
                }
            }
            else
            {
                ModelState.AddModelError("category", "You must select a category.");
            }
            AddDonationItemControlInfo(donationItem);
            return View(donationItem);
        }

        // GET: DonationItems/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            DonationItem donationItem = db.DonationItems.Include(d => d.Category).First(d => d.DonationItemId == id);
            donationItem.IsDeleted = true;
            donationItem.UpdateBy = User.Identity.GetUserName();
            if (string.IsNullOrEmpty(donationItem.Title)) donationItem.Title = "No Title";

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errors = ex.EntityValidationErrors.First().ValidationErrors.ToList();
                throw;
            }
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
            var categories = new ItemsRepository(db).GetCatalogData().Categories.Where(c => !c.IsOnlyForAuctionItems);
            var donationItemCategories = categories.Select(c => new SelectListItem { Text = c.Name, Value = c.AuctionCategoryId.ToString() }).ToList();

            if (donationItem != null && donationItem.Category != null)
            {
                var selected = donationItemCategories.FirstOrDefault(c => c.Value.Equals(donationItem.Category.AuctionCategoryId.ToString()));
                if (selected != null) selected.Selected = true;
            }            

            ViewBag.DonationItemCategories = donationItemCategories;
        }
    }
}
