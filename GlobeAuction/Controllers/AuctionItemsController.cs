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
using Microsoft.AspNet.Identity;

namespace GlobeAuction.Controllers
{
    public class AuctionItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AuctionItems
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Index()
        {
            var auctionItems = db.AuctionItems.Include(a => a.DonationItems).ToList();
            var donationItemIdsInAuctionItem = auctionItems.SelectMany(ai => ai.DonationItems.Select(di => di.DonationItemId)).ToList();
            var donationItemsNotInAuctionItem = db.DonationItems.Where(di => !di.IsDeleted && !donationItemIdsInAuctionItem.Contains(di.DonationItemId)).ToList();

            var bidderIdToNumber = db.Bidders.ToDictionary(b => b.BidderId, b => b.BidderNumber);

            var model = new ItemsViewModel
            {
                AuctionItems = auctionItems.Select(i =>
                {
                    int? bidderNumber = null;
                    if (i.WinningBidderId.HasValue)
                    {
                        if (bidderIdToNumber.ContainsKey(i.WinningBidderId.Value))
                            bidderNumber = bidderIdToNumber[i.WinningBidderId.Value];
                        else
                            bidderNumber = bidderNumber;
                    }
                    return new AuctionItemViewModel(i, bidderNumber);
                }).ToList(),
                DonationsNotInAuctionItem = donationItemsNotInAuctionItem.Select(d => new DonationItemViewModel(d)).ToList()
            };
            return View(model);
        }

        // GET: AuctionItems/Details/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            if (auctionItem == null)
            {
                return HttpNotFound();
            }
            return View(auctionItem);
        }

        // GET: AuctionItems/PrintBidSheets?auctionItemIds=1,2,3
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult PrintBidSheets(string auctionItemIds)
        {
            if (string.IsNullOrEmpty(auctionItemIds))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ids = auctionItemIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)).ToList();

            var auctionItems = db.AuctionItems.Where(ai => ids.Contains(ai.UniqueItemNumber)).ToList();

            return View(auctionItems);
        }

        // GET: AuctionItems/Edit/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            if (auctionItem == null)
            {
                return HttpNotFound();
            }
            int? bidderNumber = null;
            if (auctionItem.WinningBidderId.HasValue)
            {
                bidderNumber = db.Bidders.First(b => b.BidderId == auctionItem.WinningBidderId.Value).BidderNumber;
            }
            var viewModel = new AuctionItemViewModel(auctionItem, bidderNumber);
            AddAuctionItemControlInfo(viewModel);
            return View(viewModel);
        }

        // POST: AuctionItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Edit(AuctionItemViewModel auctionItemModel)
        {
            if (ModelState.IsValid)
            {
                var auctionItem = db.AuctionItems.FirstOrDefault(ai => ai.AuctionItemId == auctionItemModel.AuctionItemId);
                if (auctionItem == null) return HttpNotFound();

                if (auctionItem.Invoice != null)
                {
                    ModelState.AddModelError("uniqueItemNumber", "Cannot change the item once it's on an invoice.  If this is just testing you can delete the invoice to free up the item again.");
                }
                else
                {
                    int? winBidderId = null;
                    if (auctionItemModel.WinningBidderNumber.HasValue)
                    {
                        var bidder = db.Bidders.FirstOrDefault(b => b.BidderNumber == auctionItemModel.WinningBidderNumber.Value);
                        if (bidder == null)
                        {
                            ModelState.AddModelError("winningBidderNumber", "This winning bidder number is not recognized.  Make sure you are using the paddle number and not the bidder ID");
                            AddAuctionItemControlInfo(auctionItemModel);
                            return View(auctionItemModel);
                        }
                        winBidderId = bidder.BidderId;
                    }

                    //only update the fields from the model that were shown on the page
                    auctionItem.UniqueItemNumber = auctionItemModel.UniqueItemNumber;
                    auctionItem.Title = auctionItemModel.Title;
                    auctionItem.Description = auctionItemModel.Description;
                    auctionItem.Category = auctionItemModel.Category;
                    auctionItem.StartingBid = auctionItemModel.StartingBid;
                    auctionItem.BidIncrement = auctionItemModel.BidIncrement;
                    auctionItem.WinningBidderId = winBidderId;
                    auctionItem.WinningBid = auctionItemModel.WinningBid;
                    auctionItem.UpdateDate = DateTime.Now;
                    auctionItem.UpdateBy = User.Identity.GetUserName();                    
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            AddAuctionItemControlInfo(auctionItemModel);
            return View(auctionItemModel);
        }

        // POST: AuctionItems/RemoveFromBasket/5
        [HttpGet]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult RemoveFromBasket(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.FirstOrDefault(ai => ai.DonationItems.Any(di => di.DonationItemId == id));
            if (auctionItem == null)
            {
                return HttpNotFound();
            }

            var donationItem = auctionItem.DonationItems.First(di => di.DonationItemId == id);
            auctionItem.DonationItems.Remove(donationItem);
            auctionItem.UpdateDate = DateTime.Now;
            auctionItem.UpdateBy = User.Identity.GetUserName();

            db.Entry(auctionItem).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = auctionItem.AuctionItemId });
        }


        // GET: AuctionItems/Delete/5
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            if (auctionItem == null)
            {
                return HttpNotFound();
            }
            return View(auctionItem);
        }

        // POST: AuctionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult DeleteConfirmed(int id)
        {
            AuctionItem auctionItem = db.AuctionItems.Find(id);
            auctionItem.DonationItems.Clear();

            db.AuctionItems.Remove(auctionItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpPost, ActionName("SubmitSelectedDonationItems")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult SubmitSelectedDonationItems(string donationItemsAction, string selectedDonationItemIds, int? basketItemNumber, int? numberOfCopies)
        {
            var selectedDonationIds = selectedDonationItemIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse);

            if (!selectedDonationIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var selectedDonations = selectedDonationIds.Select(id => db.DonationItems.Find(id)).ToList();
            var nextUniqueId = 1;

            if (db.AuctionItems.Any())
            {
                nextUniqueId = db.AuctionItems.Max(a => a.UniqueItemNumber) + 1;
            }

            var username = User.Identity.GetUserName();
            switch (donationItemsAction)
            {
                case "MakeBasket":
                    var basket = ItemsRepository.CreateAuctionItemForDonations(nextUniqueId, selectedDonations, username);
                    db.AuctionItems.Add(basket);
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { id = basket.AuctionItemId });

                case "MakeSingle":
                    foreach (var selectedDonation in selectedDonations)
                    {
                        var auctionItem = ItemsRepository.CreateAuctionItemForDonation(nextUniqueId, selectedDonation, username);
                        db.AuctionItems.Add(auctionItem);
                        nextUniqueId++;
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index");
                case "AddToBasket":
                    if (!basketItemNumber.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var existingAuctionItem = db.AuctionItems.FirstOrDefault(ai => ai.UniqueItemNumber == basketItemNumber.Value);
                    if (existingAuctionItem == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    foreach (var selectedDonation in selectedDonations)
                    {
                        existingAuctionItem.DonationItems.Add(selectedDonation);
                    }
                    existingAuctionItem.UpdateDate = DateTime.Now;
                    existingAuctionItem.UpdateBy = username;
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { id = existingAuctionItem.AuctionItemId });
                case "DuplicateDonationItems":
                    if (!numberOfCopies.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    foreach (var selectedDonation in selectedDonations)
                    {
                        for (int i = 0; i < numberOfCopies.Value; i++)
                        {
                            db.DonationItems.Add(selectedDonation);
                            db.SaveChanges();
                        }
                    }
                    return RedirectToAction("Index");
                case "MoveDonationItemsToStore":
                    new ItemsRepository(db).CreateStoreItemsForDonations(selectedDonations, username);
                    return RedirectToAction("Index", "StoreItems");
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("SubmitSelectedAuctionItems")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult SubmitSelectedAuctionItems(string postActionName, string selectedAuctionItemIds, int? startingAuctionItemNumber)
        {
            var selectedAuctionIds = selectedAuctionItemIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            if (!selectedAuctionIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var selectedAuctionItems = db.AuctionItems.Where(ai => selectedAuctionIds.Contains(ai.UniqueItemNumber)).ToList();
            if (!selectedAuctionItems.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var username = User.Identity.GetUserName();
            switch (postActionName)
            {
                case "RenumberAuctionItems":
                    var nextItemNum = startingAuctionItemNumber.GetValueOrDefault(0);
                    if (nextItemNum <= 0)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    foreach (var auctionItem in selectedAuctionItems.OrderBy(a => a.UniqueItemNumber))
                    {
                        auctionItem.UniqueItemNumber = nextItemNum;
                        auctionItem.UpdateBy = username;
                        auctionItem.UpdateDate = DateTime.Now;
                        nextItemNum++;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        [Authorize(Roles = AuctionRoles.CanEditWinners + "," + AuctionRoles.CanCheckoutWinners)]
        public ActionResult Winners()
        {
            var winningsByBidder = new ItemsRepository(db).GetWinningsByBidder();
            var models = winningsByBidder.Select(wbb => new WinnerViewModel(wbb.Bidder, wbb.Winnings)).ToList();
            return View(models);
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult PrintAllPackSlips()
        {
            var winningsByBidder = new ItemsRepository(db).GetWinningsByBidder();
            var models = winningsByBidder.Select(wbb => new WinnerViewModel(wbb.Bidder, wbb.Winnings)).ToList();
            return View(models);
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult EmailAllWinners()
        {
            var winningsByBidder = new ItemsRepository(db).GetWinningsByBidder();
            var winnersToEmail = winningsByBidder.Where(w => w.AreWinningsAllPaidFor == false && w.Bidder.IsCheckoutNudgeEmailSent == false).ToList();

            var model = new NotifyResultViewModel();
            DoEmailWinners(winnersToEmail, model, false);

            return View(model);
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult EmailUnpaidWinnersAfterEvent()
        {
            var winningsByBidder = new ItemsRepository(db).GetWinningsByBidder();
            var winnersToEmail = winningsByBidder.Where(w => w.AreWinningsAllPaidFor == false).ToList();

            var model = new NotifyResultViewModel();
            DoEmailWinners(winnersToEmail, model, true);

            return View(model);
        }

        private void DoEmailWinners(List<WinningsByBidder> winnersToEmail, NotifyResultViewModel model, bool isAfterEvent)
        {
            var emailHelper = new EmailHelper();
            foreach (var winner in winnersToEmail) //only email people with outstanding unpaid winnings
            {
                try
                {
                    var payLink = Url.Action("ReviewBidderWinnings", "Invoices", new { bid = winner.Bidder.BidderId, email = winner.Bidder.Email }, Request.Url.Scheme);
                    emailHelper.SendAuctionWinningsPaymentNudge(winner.Bidder, winner.Winnings, payLink, isAfterEvent);
                    model.MessagesSent++;

                    if (!isAfterEvent)
                    {
                        winner.Bidder.IsCheckoutNudgeEmailSent = true;
                        db.SaveChanges();
                    }
                }
                catch (Exception exc)
                {
                    model.MessagesFailed++;
                    model.ErrorMessage = exc.Message;
                }
            }
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult TextAllWinners()
        {
            var winningsByBidder = new ItemsRepository(db).GetWinningsByBidder();
            var txtHelper = new SmsHelper();
            var urlHelper = new TinyUrlHelper();
            var winnersToNotify = winningsByBidder.Where(w => w.AreWinningsAllPaidFor == false && w.Bidder.IsCheckoutNudgeTextSent == false).ToList();

            var model = new NotifyResultViewModel();
            foreach (var winner in winnersToNotify)
            {
                //for (int i = 0; i < 100; i++)
                //{
                try
                {
                    var payLink = Url.Action("ReviewBidderWinnings", "Invoices", new { bid = winner.Bidder.BidderId, email = winner.Bidder.Email }, Request.Url.Scheme);
                    var tinyUrl = urlHelper.GetTinyUrl(payLink);
                    var body = "You won GLOBE Auction items!  Click here to checkout: " + tinyUrl;
                    txtHelper.SendSms(winner.Bidder.Phone, body);
                    model.MessagesSent++;
                    winner.Bidder.IsCheckoutNudgeTextSent = true;
                    db.SaveChanges();
                }
                catch (Exception exc)
                {
                    model.MessagesFailed++;
                    model.ErrorMessage = exc.Message;
                }
                //}
            }

            return View(model);
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult EnterWinners()
        {
            AddAuctionItemCategoryControlInfo(null);

            return View(new EnterWinnersViewModel());
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult GetNextAuctionItemWithNoWinner(string selectedCategory, int currentUniqueItemNumber)
        {
            var nextItem = db.AuctionItems.Where(ai =>
                    ai.WinningBidderId.HasValue == false &&
                    ai.Category == selectedCategory &&
                    ai.UniqueItemNumber > currentUniqueItemNumber)
                .OrderBy(ai => ai.UniqueItemNumber)
                .FirstOrDefault();

            if (nextItem == null)
            {
                return Json(new { hasNext = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(
                new
                {
                    hasNext = true,
                    auctionItemId = nextItem.AuctionItemId,
                    title = nextItem.Title,
                    description = nextItem.Description,
                    uniqueItemNumber = nextItem.UniqueItemNumber
                },
                JsonRequestBehavior.AllowGet
            );
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult SaveAuctionItemWinner(int auctionItemId, int uniqueItemNumber, string winningBidderId, string winningAmount)
        {
            int winningBidderIdInt;
            decimal winningAmountDecimal;

            if (!int.TryParse(winningBidderId, out winningBidderIdInt))
            {
                return Json(new { wasSuccessful = false, errorMsg = "Winning Bidder # must be a whole number." }, JsonRequestBehavior.AllowGet);
            }
            if (!decimal.TryParse(winningAmount, out winningAmountDecimal))
            {
                return Json(new { wasSuccessful = false, errorMsg = "Winning Bid Amount must be a whole number." }, JsonRequestBehavior.AllowGet);
            }

            var auctionItem = db.AuctionItems.FirstOrDefault(ai =>
                ai.AuctionItemId == auctionItemId &&
                ai.UniqueItemNumber == uniqueItemNumber);

            if (auctionItem == null)
            {
                return Json(new { wasSuccessful = false, errorMsg = "Unable to find auction item." }, JsonRequestBehavior.AllowGet);
            }
            if (auctionItem.WinningBidderId.HasValue || auctionItem.WinningBid.HasValue)
            {
                return Json(new { wasSuccessful = false, errorMsg = "Auction Item is already marked as won.  You must use the Auction Item edit screen to update this now." }, JsonRequestBehavior.AllowGet);
            }

            var bidder = db.Bidders.FirstOrDefault(b => b.IsDeleted == false && b.BidderId == winningBidderIdInt);

            if (bidder == null)
            {
                return Json(new { wasSuccessful = false, errorMsg = "Unable to find bidder " + winningBidderIdInt + "." }, JsonRequestBehavior.AllowGet);
            }

            auctionItem.WinningBid = winningAmountDecimal;
            auctionItem.WinningBidderId = bidder.BidderId;
            auctionItem.UpdateBy = User.Identity.GetUserName();
            auctionItem.UpdateDate = DateTime.Now;
            db.SaveChanges();

            return Json(new { wasSuccessful = true }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddAuctionItemControlInfo(AuctionItemViewModel item)
        {
            AddAuctionItemCategoryControlInfo(item != null ? item.Category : null);
        }

        private void AddAuctionItemCategoryControlInfo(string selectedCategory)
        {
            var auctionItemCategories = AuctionConstants.DonationItemCategories.Select(c => new SelectListItem { Text = c, Value = c }).ToList();

            //additional categories for auction items
            auctionItemCategories.Add(new SelectListItem { Text = "Live", Value = "Live" });

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                var selected = auctionItemCategories.FirstOrDefault(c => c.Value.Equals(selectedCategory));
                if (selected != null) selected.Selected = true;
            }


            ViewBag.AuctionItemCategories = auctionItemCategories;
        }
    }
}
