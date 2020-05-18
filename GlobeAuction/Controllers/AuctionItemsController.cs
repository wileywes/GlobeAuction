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
using System.IO;

namespace GlobeAuction.Controllers
{
    public class AuctionItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AuctionItems
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Index()
        {
            var auctionItems = db.AuctionItems
                .Include(a => a.DonationItems)
                .Include(a => a.AllBids)
                .Include(a => a.Category)
                .ToList();

            var donationItemIdsInAuctionItem = auctionItems.SelectMany(ai => ai.DonationItems.Select(di => di.DonationItemId)).ToList();

            var donationItemsNotInAuctionItem = db.DonationItems
                .Include(a => a.Category)
                .Where(di => !donationItemIdsInAuctionItem.Contains(di.DonationItemId))
                .ToList();

            var donationItemsOnStore = donationItemsNotInAuctionItem.Where(i => i.IsInStore).ToList();
            var donationItemsFreeForAuctionItems = donationItemsNotInAuctionItem.Where(i => !i.IsDeleted).ToList();

            var bidderIdToNumber = db.Bidders.ToDictionary(b => b.BidderId, b => b.BidderNumber);

            var model = new ItemsViewModel
            {
                AuctionItems = auctionItems.Select(i => new AuctionItemViewModel(i)).ToList(),
                DonationsNotInAuctionItem = donationItemsFreeForAuctionItems.Select(d => new DonationItemViewModel(d)).ToList(),
                DonationsInStore = donationItemsOnStore.Select(d => new DonationItemViewModel(d)).ToList()
            };
            return View(model);
        }

        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult Firesale()
        {
            var auctionItems = db.AuctionItems
                .Include(a => a.DonationItems)
                .Include(a => a.AllBids)
                .Include(a => a.Category)
                .Where(ai => ai.Quantity > ai.AllBids.Count)
                .ToList();
                        
            var model = new FiresaleItemsViewModel
            {
                AuctionItems = auctionItems.Select(i => new FiresaleItemViewModel(i)).ToList()
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
            var viewModel = new AuctionItemViewModel(auctionItem);
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

                var category = db.AuctionCategories.FirstOrDefault(c => c.Name == auctionItemModel.Category);

                if (auctionItem.AllBids.Any(b => b.IsPaid))
                {
                    ModelState.AddModelError("uniqueItemNumber", "Cannot change the item once it's on an invoice.  If this is just testing you can delete the invoice to free up the item again.");
                }
                else if (category == null)
                {
                    ModelState.AddModelError("category", "You must select a valid auction item category.");
                }
                else
                {
                    //only update the fields from the model that were shown on the page
                    auctionItem.UniqueItemNumber = auctionItemModel.UniqueItemNumber;
                    auctionItem.Title = auctionItemModel.Title;
                    auctionItem.Description = auctionItemModel.Description;
                    auctionItem.ImageUrl = auctionItemModel.ImageUrl;
                    auctionItem.Category = category;
                    auctionItem.StartingBid = auctionItemModel.StartingBid;
                    auctionItem.BidIncrement = auctionItemModel.BidIncrement;
                    auctionItem.IsFixedPrice = auctionItemModel.IsFixedPrice;
                    auctionItem.UpdateDate = Utilities.GetEasternTimeNow();
                    auctionItem.UpdateBy = User.Identity.GetUserName();
                    auctionItem.Quantity = auctionItemModel.Quantity;
                    db.SaveChanges();

                    new ItemsRepository(db).ClearCatalogDataCache();

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
            auctionItem.UpdateDate = Utilities.GetEasternTimeNow();
            auctionItem.UpdateBy = User.Identity.GetUserName();
            
            //recalc starting and increment with new items list
            int startingBid, bidIncrement;
            ItemsRepository.CalculateStartBidAndIncrement(auctionItem.DonationItems, out startingBid, out bidIncrement);
            auctionItem.StartingBid = startingBid;
            auctionItem.BidIncrement = bidIncrement;

            db.Entry(auctionItem).State = EntityState.Modified;
            db.SaveChanges();
            new ItemsRepository(db).ClearCatalogDataCache();
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
            new ItemsRepository(db).ClearCatalogDataCache();
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

            var selectedDonations = db.DonationItems
                .Include(d => d.Category)
                .Where(i => selectedDonationIds.Contains(i.DonationItemId))
                .ToList();
            
            new ItemsRepository(db).ClearCatalogDataCache();
            var username = User.Identity.GetUserName();
            switch (donationItemsAction)
            {
                case "MakeBasket":
                    var basket = new ItemsRepository(db).CreateAuctionItemForDonations(selectedDonations, username);
                    db.AuctionItems.Add(basket);
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { id = basket.AuctionItemId });

                case "MakeSingle":
                    foreach (var selectedDonation in selectedDonations)
                    {
                        var auctionItem = new ItemsRepository(db).CreateAuctionItemForDonation(selectedDonation, username);
                        db.AuctionItems.Add(auctionItem);
                        db.SaveChanges(); //save between each so that the next item calculates the correct Item No.
                    }
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
                    existingAuctionItem.UpdateDate = Utilities.GetEasternTimeNow();
                    existingAuctionItem.UpdateBy = username;

                    int startingBid, bidIncrement;
                    ItemsRepository.CalculateStartBidAndIncrement(existingAuctionItem.DonationItems, out startingBid, out bidIncrement);
                    existingAuctionItem.StartingBid = startingBid;
                    existingAuctionItem.BidIncrement = bidIncrement;

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
                case "UseDigitalCertificateForWinner":
                    foreach (var selectedDonation in selectedDonations)
                    {
                        selectedDonation.UseDigitalCertificateForWinner = true;
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index");
                case "MarkAsReceived":
                    foreach (var selectedDonation in selectedDonations)
                    {
                        selectedDonation.IsReceived = true;
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index");
                default:
                    throw new ApplicationException("Unrecognized donation item action");
            }
        }

        [HttpPost, ActionName("SubmitSelectedAuctionItems")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult SubmitSelectedAuctionItems(string auctionItemsAction, string selectedAuctionItemIds, int? startingAuctionItemNumber, int? auctionItemIdForUpload, IEnumerable<HttpPostedFileBase> files)
        {
            var selectedAuctionIds = selectedAuctionItemIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            new ItemsRepository(db).ClearCatalogDataCache();
            var username = User.Identity.GetUserName();
            switch (auctionItemsAction)
            {
                case "RenumberAuctionItems":
                    if (!selectedAuctionIds.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var selectedAuctionItems = db.AuctionItems.Where(ai => selectedAuctionIds.Contains(ai.UniqueItemNumber)).ToList();
                    if (!selectedAuctionItems.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var nextItemNum = startingAuctionItemNumber.GetValueOrDefault(0);
                    if (nextItemNum <= 0)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    foreach (var auctionItem in selectedAuctionItems.OrderBy(a => a.UniqueItemNumber))
                    {
                        auctionItem.UniqueItemNumber = nextItemNum;
                        auctionItem.UpdateBy = username;
                        auctionItem.UpdateDate = Utilities.GetEasternTimeNow();
                        nextItemNum++;
                    }
                    db.SaveChanges();
                    break;
                case "UploadImage":
                    //image upload
                    const string pathBase = "~/Content/images/AuctionItems";
                    if (!auctionItemIdForUpload.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var auctionItemForImage = db.AuctionItems.Find(auctionItemIdForUpload.Value);
                    if (auctionItemForImage == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var file = files.FirstOrDefault(f => f != null && f.ContentLength > 0);
                    if (file == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath(pathBase), fileName);
                    file.SaveAs(path);

                    auctionItemForImage.ImageUrl = pathBase + "/" + fileName;
                    db.SaveChanges();
                    break;
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("SubmitSelectedFiresaleItems")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditItems)]
        public ActionResult SubmitSelectedFiresaleItems(string auctionItemsAction, string selectedAuctionItemIds)
        {
            var selectedAuctionIds = selectedAuctionItemIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            new ItemsRepository(db).ClearCatalogDataCache();
            var username = User.Identity.GetUserName();
            switch (auctionItemsAction)
            {
                case "PutItemsInFiresale":
                    if (!selectedAuctionIds.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var selectedAuctionItems = db.AuctionItems.Where(ai => selectedAuctionIds.Contains(ai.UniqueItemNumber)).ToList();
                    if (!selectedAuctionItems.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    foreach (var auctionItem in selectedAuctionItems.OrderBy(a => a.UniqueItemNumber))
                    {
                        auctionItem.IsInFiresale = true;
                        auctionItem.UpdateBy = username;
                        auctionItem.UpdateDate = Utilities.GetEasternTimeNow();
                    }
                    db.SaveChanges();
                    break;
            }

            return RedirectToAction("Firesale");
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners + "," + AuctionRoles.CanCheckoutWinners)]
        public ActionResult Winners()
        {
            var winningsByBidder = new ItemsRepository(db).GetWinningsByBidder();
            var models = winningsByBidder.Select(wbb => new WinnerViewModel(wbb.Bidder, wbb.Winnings)).ToList();
            return View(models);
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners + "," + AuctionRoles.CanCheckoutWinners)]
        public ActionResult PrintAllPackSlips(bool? useMockData, bool? onlyPhysicalItems)
        {
            if (User.Identity.GetUserName() != "williams.wes@gmail.com")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            List<WinningsByBidder> winningsByBidder;
            if (useMockData.GetValueOrDefault())
            {
                winningsByBidder = new ItemsRepository(db).Mock_GetWinningsByBidder();
            }
            else
            {
                winningsByBidder = new ItemsRepository(db).GetWinningsByBidder();
            }

            if (onlyPhysicalItems.GetValueOrDefault(false))
            {
                winningsByBidder = winningsByBidder.Where(w => w.HasPhysicalWinnings).ToList();
            }

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
            var emailHelper = EmailHelperFactory.Instance();
            foreach (var winner in winnersToEmail) //only email people with outstanding unpaid winnings
            {
                //for (int i = 0; i < 200; i++)
                //{
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
                //}
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
                //for (int i = 0; i < 200; i++)
                //{
                try
                {
                    var payLink = Url.Action("ReviewBidderWinnings", "Invoices", new { bid = winner.Bidder.BidderId, email = winner.Bidder.Email }, Request.Url.Scheme);
                    //var tinyUrl = urlHelper.GetTinyUrl(payLink);
                    var body = "You won GLOBE Auction items!  Click here to checkout: " + payLink;
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
            var nextItems = db.AuctionItems.Where(ai =>
                    ai.AllBids.Count == 0 &&
                    ai.Quantity == 1 &&
                    ai.Category.Name == selectedCategory &&
                    ai.UniqueItemNumber > currentUniqueItemNumber)
                .OrderBy(ai => ai.UniqueItemNumber)
                .ToList();

            var nextItem = nextItems.FirstOrDefault();

            var hasNext = nextItems.Count > 1;
            var hasPrevious = db.AuctionItems.Any(ai =>
                    ai.AllBids.Count == 0 &&
                    ai.Quantity == 1 &&
                    ai.Category.Name == selectedCategory &&
                    ai.UniqueItemNumber <= currentUniqueItemNumber);

            if (nextItem == null)
            {
                return Json(new { hasResult = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(
                new
                {
                    hasResult = true,
                    hasNext,
                    hasPrevious,
                    auctionItemId = nextItem.AuctionItemId,
                    title = nextItem.Title,
                    description = nextItem.Description,
                    uniqueItemNumber = nextItem.UniqueItemNumber
                },
                JsonRequestBehavior.AllowGet
            );
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult GetPreviousAuctionItemWithNoWinner(string selectedCategory, int currentUniqueItemNumber)
        {
            var previousItems = db.AuctionItems.Where(ai =>
                    ai.AllBids.Count == 0 &&
                    ai.Quantity == 1 &&
                    ai.Category.Name == selectedCategory &&
                    ai.UniqueItemNumber < currentUniqueItemNumber)
                .OrderByDescending(ai => ai.UniqueItemNumber)
                .ToList();

            var previousItem = previousItems.FirstOrDefault();
            var hasPrevious = previousItems.Count > 1;
            var hasNext = db.AuctionItems.Any(ai =>
                    ai.AllBids.Count == 0 &&
                    ai.Quantity == 1 &&
                    ai.Category.Name == selectedCategory &&
                    ai.UniqueItemNumber >= currentUniqueItemNumber);

            if (previousItem == null)
            {
                return Json(new { hasResult = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(
                new
                {
                    hasResult = true,
                    hasNext,
                    hasPrevious,
                    auctionItemId = previousItem.AuctionItemId,
                    title = previousItem.Title,
                    description = previousItem.Description,
                    uniqueItemNumber = previousItem.UniqueItemNumber
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
            if (auctionItem.AllBids.Count > 0)
            {
                return Json(new { wasSuccessful = false, errorMsg = "Auction Item already has a winning bid on it.  You must use the Auction Item edit screen to delete that bid if that is not correct." }, JsonRequestBehavior.AllowGet);
            }
            if (auctionItem.Quantity > 1)
            {
                return Json(new { wasSuccessful = false, errorMsg = "Cannot assign a winner to an auction item that allows multiple winners.  Use the bulk winner entry screen instead." }, JsonRequestBehavior.AllowGet);
            }

            var bidder = db.Bidders.FirstOrDefault(b => b.IsDeleted == false && b.BidderNumber == winningBidderIdInt);

            if (bidder == null)
            {
                return Json(new { wasSuccessful = false, errorMsg = "Unable to find bidder " + winningBidderIdInt + "." }, JsonRequestBehavior.AllowGet);
            }

            List<Bidder> biddersThatLost;
            new ItemsRepository(db).EnterNewBidAndRecalcWinners(auctionItem, bidder, winningAmountDecimal, out biddersThatLost);

            return Json(new { wasSuccessful = true }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult EnterWinnersInBulk()
        {
            var model = new EnterWinnersInBulkViewModel();
            AddWinnersInBulkInfo(model);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult EnterWinnersInBulk(EnterWinnersInBulkViewModel model)
        {
            var item = db.AuctionItems.Find(model.SelectedAuctionItemId);
            if (item == null)
            {
                ModelState.AddModelError("uniqueItemNumber", "Master item not found.  No winners were recorded.  Please confirm your data and try again.");
            }
            else if (model.BidPrice == 0)
            {
                ModelState.AddModelError("bidPrice", "Bid Price must be greater than zero.  No winners were recorded.  Please confirm your data and try again.");
            }
            else if (string.IsNullOrEmpty(model.ListOfBidderNumbers))
            {
                ModelState.AddModelError("listOfBidderNumbers", "No bidder paddle numbers were entered.  No winners were recorded.  Please confirm your data and try again.");
            }
            else
            {
                model.ErrorMessages = new List<string>();
                model.ItemsCreated = 0;

                var bidderNumbersStrings = model.ListOfBidderNumbers.Split(new[] { Environment.NewLine, "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach(var bidNumStr in bidderNumbersStrings)
                {
                    int bidNumber = 0;
                    if (int.TryParse(bidNumStr, out bidNumber))
                    {
                        string errMsg;
                        if (TryCreateBidForBulkWinner(item, bidNumber, model.BidPrice, out errMsg))
                        {
                            model.ItemsCreated++;
                        }
                        else
                        {
                            model.ErrorMessages.Add($"Unable to create new bid for bidder number [{bidNumStr}].  This one was skipped but the rest were entered. Error was: {errMsg}");
                        }
                    }
                    else
                    {
                        model.ErrorMessages.Add($"Unable to parse bidder number [{bidNumStr}].  This one was skipped but the rest were entered.");
                    }
                }
            }

            AddWinnersInBulkInfo(model);
            return View(model);
        }

        private bool TryCreateBidForBulkWinner(AuctionItem item, int bidderNumber, decimal bidPrice, out string errorMessage)
        {
            var bidder = db.Bidders.FirstOrDefault(b => b.BidderNumber == bidderNumber && b.IsDeleted == false);
            if (bidder == null)
            {
                errorMessage = "Cannot find bidder number " + bidderNumber;
                return false;
            }

            var quantityLeft = item.Quantity - item.AllBids.Count;
            if (quantityLeft <= 0)
            {
                errorMessage = "No more quantity is left on the auction item.  If this item is unlimited quantity just increase it.";
                return false;
            }

            var newBid = new Bid
            {
                AuctionItem = item,
                BidAmount = bidPrice,
                Bidder = bidder,
                IsWinning = true,
                UpdateBy = User.Identity.GetUserName()
            };
            newBid.CreateDate = newBid.UpdateDate = Utilities.GetEasternTimeNow();

            item.AllBids.Add(newBid);
            db.SaveChanges();

            errorMessage = string.Empty;
            return true;
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult GetAuctionItemInfo(int auctionItemId)
        {
            var item = db.AuctionItems.Find(auctionItemId);

            if (item == null)
            {
                return Json(new { hasResult = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(
                new
                {
                    hasResult = true,
                    auctionItemId = item.AuctionItemId,
                    title = item.Title,
                    description = item.Description,
                    uniqueItemNumber = item.UniqueItemNumber
                },
                JsonRequestBehavior.AllowGet
            );
        }

        [AllowAnonymous]
        public ActionResult Catalog(CatalogViewModel model)
        {
            var catData = new ItemsRepository(db).GetCatalogData();
            model.AuctionItems = catData.AuctionItems;
            model.Categories = catData.Categories;
            model.TotalFiresaleCount = model.AuctionItems.Where(i => i.IsInFiresale).Count();

            if (!string.IsNullOrEmpty(model.SelectedCategory))
            {
                if (model.SelectedCategory.Equals("#firesale#"))
                {
                    model.AuctionItems = model.AuctionItems.Where(i => i.IsInFiresale).ToList();
                }
                else
                {
                    model.AuctionItems = model.AuctionItems.Where(i => i.Category.Name == model.SelectedCategory).ToList();
                }
            }

            if (!string.IsNullOrEmpty(model.SearchString))
            {
                model.AuctionItems = model.AuctionItems
                    .Where(i => i.Title.IndexOf(model.SearchString, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                i.Description.IndexOf(model.SearchString, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                i.UniqueItemNumber.ToString().IndexOf(model.SearchString, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();
            }

            if (model.FilterToItemsNoBids)
            {
                model.AuctionItems = model.AuctionItems.Where(i => i.BidCount == 0).ToList();
            }

            BidderCookieInfo bidderInfo;
            if (BidderRepository.TryGetBidderInfoFromCookie(out bidderInfo))
            {
                model.IsBidderLoggedIn = true;
                model.BidderCatalogFavoriteAuctionItemIds = new BidderRepository(db).GetBidderCatalogFavoriteAuctionItemIds(bidderInfo.BidderId, bidderInfo.BidderNumber);
            }

            //sort by category then item no
            if (model.SortByPrice)
            {
                model.AuctionItems = model.AuctionItems.OrderBy(a => a.HighestBid).ToList();
            }
            else
            {
                model.AuctionItems = model.AuctionItems.OrderBy(a => a.Category.Name).ThenBy(a => a.UniqueItemNumber).ToList();
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult CatalogItem(string itemNo)
        {
            AuctionItem item = null;
            int itemNoInt;
            if (int.TryParse(itemNo, out itemNoInt))
            {
                item = new ItemsRepository(db).GetItemWithAllBidInfo(itemNoInt);
            }
            if (item == null)
            {
                return RedirectToAction("Catalog");
            }
            var viewItem = new AuctionItemViewModel(item);
            return View(viewItem);
        }

        [Authorize(Roles = AuctionRoles.CanEditWinners)]
        public ActionResult DeleteBidFromItem(int aid, int bidId)
        {
            var auctionItem = db.AuctionItems.Find(aid);
            if (auctionItem == null) return HttpNotFound();

            var item = new ItemsRepository(db).GetItemWithAllBidInfo(auctionItem.UniqueItemNumber);

            var bid = item.AllBids.FirstOrDefault(b => b.BidId == bidId);
            if (bid == null)
            {
                return HttpNotFound();
            }

            var isWinning = bid.IsWinning;
            var bidder = bid.Bidder;

            db.Bids.Remove(bid);
            db.SaveChanges();

            //unpaid winning bids are counted as revenue so back it out if it was a winning bid
            if (isWinning)
            {
                RevenueHelper.IncrementTotalRevenue(-1 * bid.BidAmount);
            }

            new ItemsRepository(db).UpdateHighestBidForCachedItem(item);

            if (isWinning)
            {
                //email the team so we can track the need to find the next bidder
                var body = "The following winning bid was removed from an auction item.  Another winner could be sought out but Wes will need to manually mark the other as winning:<br/><br/>" +
                    $"<b>Item #:</b> {item.UniqueItemNumber}<br />" +
                    $"<b>Item Title:</b> {item.Title}<br />" +
                    $"<b>Bidder Name:</b> {bidder.FirstName} {bidder.LastName}<br />" +
                    $"<b>Bidder Email:</b> {bidder.Email}<br />" +
                    $"<b>Bid Amount:</b> {bid.BidAmount:C}<br /><br />" +
                    $"You can view the other bids on the item here: {Url.Action("Details", "AuctionItems", new { id = item.AuctionItemId }, Request.Url.Scheme)}";

                EmailHelperFactory.Instance().SendEmail("auction@theglobeacademy.net", "Winning Bid Deleted - Another Winner Possible", body);
            }

            return RedirectToAction("Details", new { id = aid });
        }

        private void AddWinnersInBulkInfo(EnterWinnersInBulkViewModel model)
        {
            var availableMasterItems = db.AuctionItems
                .Where(ai => ai.Quantity > 1 && ai.Category.IsFundAProject)
                .OrderBy(ai => ai.Category.Name)
                .ThenBy(ai => ai.Title)
                .ToList();

            var selectListItems = availableMasterItems.Select(i => 
            new SelectListItem {
                Text = i.Category.Name + " - " + i.Title,
                Value = i.AuctionItemId.ToString()
            }).ToList();
            
            ViewBag.AvailableMasterItems = selectListItems;
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
            AddAuctionItemCategoryControlInfo(item?.Category);
        }

        private void AddAuctionItemCategoryControlInfo(string selectedCategoryName)
        {
            var categories = new ItemsRepository(db).GetCatalogData().Categories;
            var auctionItemCategories = categories.Select(c => new SelectListItem { Text = c.Name, Value = c.Name }).ToList();

            if (!string.IsNullOrEmpty(selectedCategoryName))
            {
                var selected = auctionItemCategories.FirstOrDefault(c => c.Text.Equals(selectedCategoryName));
                if (selected != null) selected.Selected = true;
            }

            ViewBag.AuctionItemCategories = auctionItemCategories;
        }
    }
}
