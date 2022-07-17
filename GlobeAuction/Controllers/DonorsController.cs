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
        public ActionResult Index(int? receiptSuccess, int? receiptFailed)
        {
            var viewModel = new DonorsViewModel
            {
                Donors = db.Donors.Include(d => d.DonationItems).ToList(),
                EmailTaxReceiptsResult = new NotifyResultViewModel
                {
                    MessagesSent = receiptSuccess.GetValueOrDefault(),
                    MessagesFailed = receiptFailed.GetValueOrDefault()
                }
            };
            return View(viewModel);
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
            NLog.LogManager.GetCurrentClassLogger().Info($"Starting to email donor tax receipts.");

            var donorsToEmail = db.Donors.Where(d => !d.HasTaxReceiptBeenEmailed).Include(d => d.DonationItems).ToList();

            NLog.LogManager.GetCurrentClassLogger().Info($"Found {donorsToEmail.Count} donors to check for tax receipts.");

            var model = new NotifyResultViewModel();
            var emailHelper = EmailHelperFactory.Instance();
            foreach (var donor in donorsToEmail)
            {
                EmailTaxReceiptToDonor(donor, model, emailHelper);
            }

            return View(model);
        }

        [HttpPost, ActionName("SubmitSelectedDonors")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult SubmitSelectedDonors(string donorsAction, string selectedDonorIds)
        {
            var donorIds = selectedDonorIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var resultsModel = new NotifyResultViewModel();
            switch (donorsAction)
            {
                case "EmailTaxReceipt":
                    if (!donorIds.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var donorsToEmail = db.Donors
                        .Where(d => donorIds.Contains(d.DonorId) && !d.HasTaxReceiptBeenEmailed)
                        .Include(d => d.DonationItems)
                        .ToList();

                    var emailHelper = EmailHelperFactory.Instance();
                    foreach (var donor in donorsToEmail)
                    {
                        EmailTaxReceiptToDonor(donor, resultsModel, emailHelper);
                    }
                    break;
            }

            return RedirectToAction("Index", new { receiptSuccess = resultsModel.MessagesSent, receiptFailed = resultsModel.MessagesFailed });
        }

        private void EmailTaxReceiptToDonor(Donor donor, NotifyResultViewModel model, IEmailHelper emailHelper)
        {
            try
            {
                //only include items that have value
                var itemsToInclude = donor.DonationItems.Where(d => d.DollarValue.HasValue && !d.IsDeleted).ToList();

                //skip this guy if no items had a dollar value
                if (!itemsToInclude.Any())
                {
                    NLog.LogManager.GetCurrentClassLogger().Info($"Skipping tax receipt for donor {donor.BusinessName} since no items with dollar value.");
                    return;
                }

                emailHelper.SendDonorTaxReceipt(donor, itemsToInclude);
                model.MessagesSent++;

                donor.HasTaxReceiptBeenEmailed = true;
                db.SaveChanges();

                NLog.LogManager.GetCurrentClassLogger().Info($"Sent tax receipt for donor {donor.BusinessName} to {donor.Email}.");
            }
            catch (Exception exc)
            {
                model.MessagesFailed++;
                model.ErrorMessage = exc.Message;
            }
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
