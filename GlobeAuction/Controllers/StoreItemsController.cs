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
using GlobeAuction.Helpers;

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanAdminUsers)]
    public class StoreItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public ActionResult Buy(int? iid, string fullName)
        {
            var storeItems = db.StoreItems.Where(s => s.CanPurchaseInStore && s.IsDeleted == false).ToList();
            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanEditBidders))
            {
                //remove admin-only ticket types
                storeItems = storeItems.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }
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
                var invoice = new InvoiceRepository(db).CreateInvoiceForStoreItems(buyViewModel,
                    markedManually, markedManually ? User.Identity.GetUserName() : buyViewModel.Email);

                if (markedManually)
                {
                    return RedirectToAction("Buy", "StoreItems", new { iid = invoice.InvoiceId, fullName = invoice.FirstName + " " + invoice.LastName });
                }

                return RedirectToAction("RedirectToPayPal", "Invoices", new { iid = invoice.InvoiceId, email = invoice.Email });
            }

            return View(buyViewModel);
        }

        [Authorize(Roles = AuctionRoles.CanCheckoutWinners)]
        public ActionResult LookupBidder(int bidderId, string bidderLastName)
        {
            var bidder = db.Bidders.FirstOrDefault(b => b.BidderId == bidderId && b.LastName.Equals(bidderLastName, StringComparison.OrdinalIgnoreCase));

            if (bidder == null)
            {
                return Json(new { wasFound = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(
                new
                {
                    wasFound = true,
                    bidderId = bidder.BidderId,
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
            var models = db.StoreItems.Where(s => s.IsDeleted == false).ToList().Select(i => Mapper.Map<StoreItemViewModel>(i))
                .ToList() ?? new List<StoreItemViewModel>();
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
            var storeItem = db.StoreItems.Find(id);
            storeItem.IsDeleted = true;
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
