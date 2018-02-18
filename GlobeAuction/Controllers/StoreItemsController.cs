﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using GlobeAuction.Models;
using Microsoft.AspNet.Identity;
using GlobeAuction.Helpers;
using System.Web;
using System.IO;

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanEditItems)]
    public class StoreItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public ActionResult Buy(int? iid, string fullName)
        {
            var storeItems = db.StoreItems
                .Where(s => s.CanPurchaseInStore && s.IsDeleted == false)
                .ToList();

            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanEditBidders))
            {
                //remove admin-only ticket types
                storeItems = storeItems.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }

            //filter out items with no more quantity
            storeItems = storeItems
                .Where(si => si.IsRaffleTicket || si.Quantity > 0)
                .ToList();

            var availableStoreItems = storeItems.Select(i => Mapper.Map<StoreItemViewModel>(i)).ToList();

            var viewModel = new BuyViewModel()
            {
                StoreItemPurchases = availableStoreItems.Select(si => new StoreItemPurchaseViewModel
                {
                    StoreItem = si
                }).ToList()
            };

            //if we just created an invoice then show the info
            if (iid.HasValue)
            {
                viewModel.ShowInvoiceCreatedSuccessMessage = true;
                viewModel.InvoiceIdCreated = iid.Value;
                viewModel.InvoiceFullNameCreated = fullName;
            };

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Buy(BuyViewModel buyViewModel, string submitButton)
        {
            if (ModelState.IsValid)
            {
                var markedManually = submitButton == "Invoice and Mark Paid";

                try
                {
                    var invoice = new InvoiceRepository(db).CreateInvoiceForStoreItems(buyViewModel,
                        markedManually, markedManually ? User.Identity.GetUserName() : buyViewModel.Email);

                    if (markedManually)
                    {
                        return RedirectToAction("Buy", "StoreItems", new { iid = invoice.InvoiceId, fullName = invoice.FirstName + " " + invoice.LastName });
                    }

                    return RedirectToAction("RedirectToPayPal", "Invoices", new { iid = invoice.InvoiceId, email = invoice.Email });
                }
                catch (OutOfStockException oosExc)
                {
                    ModelState.AddModelError("", $"Item \"{oosExc.StoreItem.Title}\" is no longer available.  Please refresh this page and try your purchase again.");
                }
            }

            return View(buyViewModel);
        }

        [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
        public ActionResult LookupBidder(int bidderNumber, string bidderLastName)
        {
            var bidder = db.Bidders.FirstOrDefault(b => b.BidderNumber == bidderNumber && b.LastName.Equals(bidderLastName, StringComparison.OrdinalIgnoreCase));

            if (bidder == null)
            {
                return Json(new { wasFound = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(
                new
                {
                    wasFound = true,
                    bidderId = bidder.BidderId,
                    bidderNumber = bidder.BidderNumber,
                    firstName = bidder.FirstName,
                    lastName = bidder.LastName,
                    phone = bidder.Phone,
                    email = bidder.Email,
                    zip = bidder.ZipCode,
                },
                JsonRequestBehavior.AllowGet
            );
        }

        // GET: StoreItems
        public ActionResult Index()
        {
            var purchases = db.StoreItemPurchases.ToList();

            var models = db.StoreItems
                .Where(s => s.IsDeleted == false)
                .Include(s => s.BundleComponents)
                .ToList() //evaluate the DB query first before moving into model handling
                .Select(i =>
                {
                    var listItem = Mapper.Map<StoreItemsListViewModel>(i);
                    var itemPurchases = purchases.Where(sip => sip.StoreItem.StoreItemId == i.StoreItemId).ToList();
                    listItem.UnpaidPurchaseCount = itemPurchases.Where(sip => !sip.IsPaid).Count();
                    listItem.PaidPurchaseCount = itemPurchases.Where(sip => sip.IsPaid).Count();
                    return listItem;
                })
                .ToList();

            return View(models);
        }

        // GET: StoreItems/Purchases
        [Authorize]
        public ActionResult Purchases()
        {
            var models = db.StoreItemPurchases.ToList()
                .Select(i => Mapper.Map<StoreItemPurchaseViewModel>(i))
                .ToList();
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
            var model = new StoreItemViewModel();
            model.BundleComponents = Enumerable.Repeat(new BundleComponent(), 5).ToList();
            return View(model);
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
                ApplyBundleComponentsFromViewToModel(storeItemViewModel, storeItem);

                if (storeItem.IsBundleParent)
                {
                    //make sure prices add up
                    var totalBundlePrices = storeItem.BundleComponents.Sum(c => c.ComponentUnitPrice * c.Quantity);
                    var itemPrice = storeItem.Price;
                    if (totalBundlePrices != itemPrice)
                    {
                        ModelState.AddModelError("price", $"The sum of the price of components must equal the total price of the item.  Total of components is {totalBundlePrices:C} while item price is {itemPrice:C}");
                        return View(storeItemViewModel);
                    }
                }

                db.StoreItems.Add(storeItem);
                db.SaveChanges();

                //point components to store item after save
                if (storeItem.IsBundleParent)
                {
                    storeItem.BundleComponents.ForEach(c => c.BundleParent = storeItem);
                }
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

        private void ApplyBundleComponentsFromViewToModel(StoreItemViewModel storeItemViewModel, StoreItem storeItem)
        {
            storeItem.BundleComponents = (storeItemViewModel?.BundleComponents == null?
                new List<BundleComponent>() :
                storeItemViewModel?.BundleComponents.Where(c => c.StoreItemId > 0 && c.Quantity > 0).ToList());

            storeItem.IsBundleParent = storeItem.BundleComponents.Any();
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
                ApplyBundleComponentsFromViewToModel(storeItemViewModel, storeItem);

                //point components to store item after save
                if (storeItem.IsBundleParent)
                {
                    foreach (var comp in storeItem.BundleComponents)
                    {
                        comp.BundleParent = storeItem;                        
                    }
                }

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
            var username = User.Identity.GetUserName();
            var storeItem = db.StoreItems.Find(id);

            if (storeItem.DonationItem != null)
            {
                new ItemsRepository(db).MoveStoreItemsBackToDonations(new List<StoreItem> { storeItem }, username);
            }
            else
            {
                storeItem.IsDeleted = true;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        
        [HttpPost, ActionName("SubmitSelectedStoreItems")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult SubmitSelectedStoreItems(string storeItemsAction, string selectedStoreItemIds, int? storeItemIdForUpload, IEnumerable<HttpPostedFileBase> files)
        {
            var selectedStoreItems = selectedStoreItemIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .Select(id => db.StoreItems.Find(id))
                .ToList();

            var username = User.Identity.GetUserName();
            switch (storeItemsAction)
            {
                case "MoveToAuction":
                    if (!selectedStoreItems.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    new ItemsRepository(db).MoveStoreItemsBackToDonations(selectedStoreItems, username);
                    return RedirectToAction("Index", "AuctionItems");
                case "ShowInStore":
                    if (!selectedStoreItems.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    selectedStoreItems.ForEach(si => si.CanPurchaseInStore = true);
                    db.SaveChanges();
                    break;
                case "UploadImage":
                    //image upload
                    const string pathBase = "~/Content/images/StoreItems";
                    if (!storeItemIdForUpload.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var storeItem = db.StoreItems.Find(storeItemIdForUpload.Value);
                    if (storeItem == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var file = files.FirstOrDefault(f => f != null && f.ContentLength > 0);
                    if (file == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath(pathBase), fileName);
                    file.SaveAs(path);

                    storeItem.ImageUrl = pathBase + "/" + fileName;
                    db.SaveChanges();
                    break;
            }

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
