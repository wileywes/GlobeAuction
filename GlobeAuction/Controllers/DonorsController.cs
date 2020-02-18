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

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanAdminUsers)]
    public class DonorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Donors
        public ActionResult Index()
        {
            return View(db.Donors.Include(d => d.DonationItems).ToList());
        }

        // GET: Donors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Donor donor = db.Donors.Find(id);
            if (donor == null)
            {
                return HttpNotFound();
            }
            return View(donor);
        }

        // GET: Donors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Donor donor = db.Donors.Find(id);
            if (donor == null)
            {
                return HttpNotFound();
            }
            return View(donor);
        }

        // POST: Donors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DonorId,BusinessName,Address1,Address2,City,State,Zip,ContactName,Phone,Email,HasTaxReceiptBeenEmailed")] Donor donor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(donor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(donor);
        }
        
        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult EmailTaxReceipts()
        {
            var donorsToEmail = db.Donors.Where(d => !d.HasTaxReceiptBeenEmailed).Include(d => d.DonationItems).ToList();

            var model = new NotifyResultViewModel();
            var emailHelper = EmailHelperFactory.Instance();
            foreach (var donor in donorsToEmail)
            {
                try
                {
                    //only include items that have value
                    var itemsToInclude = donor.DonationItems.Where(d => d.DollarValue.HasValue && !d.IsDeleted).ToList();

                    //skip this guy if no items had a dollar value
                    if (!itemsToInclude.Any()) continue;

                    emailHelper.SendDonorTaxReceipt(donor, itemsToInclude);
                    model.MessagesSent++;
                    
                    donor.HasTaxReceiptBeenEmailed = true;
                    db.SaveChanges();
                }
                catch (Exception exc)
                {
                    model.MessagesFailed++;
                    model.ErrorMessage = exc.Message;
                }
            }

            return View(model);
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
