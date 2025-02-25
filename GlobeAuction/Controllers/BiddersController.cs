﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;
using GlobeAuction.Models;
using Microsoft.AspNet.Identity;
using GlobeAuction.Helpers;
using System.Data.Entity.Validation;

namespace GlobeAuction.Controllers
{
    public class BiddersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bidders
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Index()
        {
            var bidders = db.Bidders.Where(b => b.IsDeleted == false).ToList();

            var invoicesForBidders = db.Invoices
                .Include(i => i.StoreItemPurchases)
                .Include(i => i.TicketPurchases)
                .Where(i => i.InvoiceType == InvoiceType.BidderRegistration)
                .ToList();

            var viewModel = new BidderListViewModel
            {
                Bidders = new List<BidderForList>()
            };

            //bidders list
            foreach (var bidder in bidders)
            {
                var invoice = invoicesForBidders.FirstOrDefault(i => i.Bidder.BidderId == bidder.BidderId);
                var bidderForList = new BidderForList(bidder, invoice);
                viewModel.Bidders.Add(bidderForList);
            }

            //ticket sales
            viewModel.TicketSales = invoicesForBidders //use materialized list already pulled
                .SelectMany(i => i.TicketPurchases)
                .GroupBy(t => t.TicketType)
                .Select(t =>
                {
                    var sales = t.ToList();
                    return new TicketSalesRollup
                    {
                        TicketType = t.Key,
                        NumberPaid= sales.Count(s => s.IsTicketPaid),
                        NumberUnPaid = sales.Count(s => s.IsTicketPaid == false),
                        TotalPaid = sales.Sum(s => s.TicketPricePaid.GetValueOrDefault()),
                        TotalUnpaid  = sales.Where(s => s.IsTicketPaid == false).Sum(s => s.TicketPrice)
                    };
                })
                .ToList();

            return View(viewModel);
        }

        // GET: Bidders/Details/5
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            var viewModel = GetBidderViewModel(bidder);
            return View(viewModel);
        }

        // GET: Bidders/Register
        [AllowAnonymous]
        public ActionResult Register(int? bid, string bem, string promoCode)
        {
            var storeItems = db.StoreItems.Where(s => s.CanPurchaseInBidderRegistration && s.IsDeleted == false).ToList();
            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanAdminUsers))
            {
                //remove admin-only ticket types
                storeItems = storeItems.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }

            AddBidderControlInfo(promoCode);
            var newBidder = new BidderRegistrationViewModel()
            {
                AuctionGuests = new List<AuctionGuestViewModel>(Enumerable.Repeat(new AuctionGuestViewModel(), 6)),
                Students = new List<StudentViewModel>(Enumerable.Repeat(new StudentViewModel(), 4)),
                ItemPurchases = storeItems.Select(si => new BuyItemViewModel(si)).ToList()
            };

            if (bid.HasValue && !string.IsNullOrEmpty(bem))
            {
                var existingJustRegistered = db.Bidders.FirstOrDefault(b => b.BidderId == bid.Value && b.Email == bem);
                if (existingJustRegistered != null)
                {
                    var registrationInvoice = new InvoiceRepository(db).GetRegistrationInvoiceForBidder(existingJustRegistered);

                    newBidder.ShowRegistrationSuccessMessage = true;
                    newBidder.BidderNumberJustRegistered = existingJustRegistered.BidderNumber;
                    newBidder.FullNameJustRegistered = existingJustRegistered.FirstName + " " + existingJustRegistered.LastName;
                    newBidder.RaffleTicketNumbersCreated = registrationInvoice?.StoreItemPurchases?
                        .Where(sip => sip.StoreItem.IsRaffleTicket)
                        .Select(sip => sip.GetRaffleDescriptionWithItemTitle())
                        .ToList() ?? new List<string>();
                }
            }

            return View(newBidder);
        }

        // POST: Bidders/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "BidderId")] BidderRegistrationViewModel bidderViewModel, string submitButton, string promoCode)
        {
            if (!ModelState.IsValid)
            {
                AddBidderControlInfo(promoCode);
                return View(bidderViewModel);
            }
            if (!this.IsCaptchaValid(""))
            {
                ModelState.AddModelError("", $"You must enter the correct Captcha string in the Input Symbols field below.");
                AddBidderControlInfo(promoCode);
                return View(bidderViewModel);
            }

            var matchingBidder = db.Bidders.FirstOrDefault(b => b.BidderNumber > 0 && b.Email == bidderViewModel.Email && b.IsDeleted == false);
            if (matchingBidder != null)
            {
                ModelState.AddModelError("", $"This email has already registered and is used for bidder number {matchingBidder.BidderNumber}.  Only one registration is allowed per email.");
                AddBidderControlInfo(promoCode);
                bidderViewModel.ShowPreviousRegistrationFoundMessage = true;
                bidderViewModel.FullNameAlreadyRegistered = matchingBidder.FirstName + " " + matchingBidder.LastName;
                bidderViewModel.BidderNumberAlreadyRegistered = matchingBidder.BidderNumber;
                bidderViewModel.BidderIdAltreadyRegistered = matchingBidder.BidderId;

                var invoiceForBidder = db.Invoices.FirstOrDefault(i => i.InvoiceType == InvoiceType.BidderRegistration && i.Bidder.BidderId == matchingBidder.BidderId);
                bidderViewModel.TicketsAlreadyPaidFor = invoiceForBidder?.TicketPurchases.Count(t => t.IsTicketPaid) ?? 0;
                return View(bidderViewModel);
            }

            try
            {
                //no lock is taken for bidder creation since the BidderNumber has a unique constraint in the DB
                var bidder = Mapper.Map<Bidder>(bidderViewModel);
                var updatedBy = User.Identity.GetUserName();
                if (string.IsNullOrEmpty(updatedBy)) updatedBy = bidder.Email;

                var biddersIdsOverZero = db.Bidders.Where(b => b.BidderNumber > 0).Select(b => b.BidderNumber).ToList();

                //default to next one up
                var nextBidderNumber = db.Bidders.Select(b => b.BidderNumber).DefaultIfEmpty(Constants.StartingBidderNumber - 1).Max() + 1;

                //fill in gaps
                var lowestExistingBidderOverZero = biddersIdsOverZero.DefaultIfEmpty(1).Min();
                for (int i = lowestExistingBidderOverZero; i < 1000; i++)
                {
                    if (biddersIdsOverZero.Contains(i) == false)
                    {
                        nextBidderNumber = i;
                        break;
                    }
                }

                bidder.BidderNumber = nextBidderNumber;
                bidder.CreateDate = bidder.UpdateDate = Utilities.GetEasternTimeNow();
                bidder.UpdateBy = updatedBy;

                //strip out dependents that weren't filled in
                bidder.Students = bidderViewModel.Students.Where(s => !string.IsNullOrEmpty(s.HomeroomTeacher)).Select(s => Mapper.Map<Student>(s)).ToList();

                if (bidderViewModel.AuctionGuests == null)
                {
                    //in 2020 we removed auction guests since there was just one free ticket, so backfill the data manually here
                    var ticketTypes = GetTicketTypesForRegistration(promoCode);
                    var ticketToUse = ticketTypes.FirstOrDefault(t => t.Name.StartsWith("2021 Auction Registration")) ?? ticketTypes.FirstOrDefault();

                    if (ticketToUse == null) throw new ApplicationException("Unable to find ticket type to use");

                    bidder.AuctionGuests = new List<AuctionGuest>
                            {
                                new AuctionGuest
                                {
                                    FirstName = bidderViewModel.FirstName,
                                    LastName = bidderViewModel.LastName,
                                    TicketType = ticketToUse.TicketTypeId.ToString()
                                }
                            };
                }
                else
                {
                    bidder.AuctionGuests = bidderViewModel.AuctionGuests.Where(g => !string.IsNullOrEmpty(g.FirstName)).Select(s => Mapper.Map<AuctionGuest>(s)).ToList();
                }

                if (bidder.AuctionGuests.Any())
                {
                    foreach (var guest in bidder.AuctionGuests)
                    {
                        var ticketType = db.TicketTypes.Find(int.Parse(guest.TicketType));
                        guest.TicketType = ticketType.Name;
                        guest.TicketPrice = ticketType.Price;
                    }

                    db.Bidders.Add(bidder);
                    db.SaveChanges();

                    PaymentMethod? manualPayMethod = null;
                    if (submitButton.StartsWith("Register and Mark Paid"))
                    {
                        if (submitButton.EndsWith("(Cash)")) manualPayMethod = PaymentMethod.Cash;
                        if (submitButton.EndsWith("(Check)")) manualPayMethod = PaymentMethod.Check;
                        if (submitButton.EndsWith("(PayPal)")) manualPayMethod = PaymentMethod.PayPalHere;
                    }

                    var invoice = new InvoiceRepository(db).CreateInvoiceForBidderRegistration(bidder, bidderViewModel, manualPayMethod, updatedBy);

                    if (manualPayMethod.HasValue || invoice.IsPaid)
                    {
                        EmailHelperFactory.Instance().SendBidderRegistrationConfirmationOrPaddleReminder(bidder);

                        return RedirectToAction("Register", new { bid = bidder.BidderId, bem = bidder.Email });
                    }
                    else
                    {
                        return RedirectToAction("RedirectToPayPal", new { id = bidder.BidderId });
                    }
                }
                else
                {
                    ModelState.AddModelError("", $"You must register a least one guest.  If you have any questions please contact ptccglobeauction@gmail.com.");
                }
            }
            catch (OutOfStockException oosExc)
            {
                ModelState.AddModelError("", $"Item \"{oosExc.StoreItem.Title}\" is no longer available.  Please refresh this page and try your registration again (you have not been charged yet).");
            }

            AddBidderControlInfo(promoCode);
            return View(bidderViewModel);
        }

        [AllowAnonymous]
        public ActionResult RedirectToPayPal(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            //before we redirect, make sure we have the latest prices on the current tickets configuration
            var ticketTypes = db.TicketTypes.ToList();
            var changesMade = false;
            foreach (var guest in bidder.AuctionGuests)
            {
                var matchingTT = ticketTypes.FirstOrDefault(t => t.Name.Equals(guest.TicketType));
                if (matchingTT != null && matchingTT.Price != guest.TicketPrice)
                {
                    guest.TicketPrice = matchingTT.Price;
                    changesMade = true;
                }
            }

            if (changesMade)
                db.SaveChanges();

            var invoice = new InvoiceRepository(db).GetRegistrationInvoiceForBidder(bidder);
            if (invoice == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = new BidderForPayPal(bidder, invoice);

            return View(viewModel);
        }

        [AllowAnonymous]
        //[HttpPost, HttpGet]
        public ActionResult PayPalComplete(int? bid, int? iid, FormCollection form)
        {
            Bidder bidder;
            if (form != null && form.AllKeys.Count() > 0 && form["txn_id"] != null)
            {
                var ppTrans = new PayPalTransaction(form);
                db.PayPalTransactions.Add(ppTrans);
                db.SaveChanges(); //go ahead and record the transaction

                var bidderId = BidderRepository.GetBidderIdFromTransaction(ppTrans);
                if (!bidderId.HasValue)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                bidder = db.Bidders.Find(bidderId.Value);
                if (bidder == null)
                {
                    return HttpNotFound();
                }

                var invoiceRepos = new InvoiceRepository(db);
                var regInvoice = invoiceRepos.GetRegistrationInvoiceForBidder(bidder);
                invoiceRepos.ApplyPaymentToInvoice(ppTrans, regInvoice);
                NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for bidder " + bidder.BidderId + " via cart post-back");
            }
            else if (bid.HasValue && iid.HasValue)
            {
                bidder = db.Bidders.Find(bid.Value);
                if (bidder == null)
                {
                    return HttpNotFound();
                }
                NLog.LogManager.GetCurrentClassLogger().Info($"Bidder id:{bidder.BidderId} lname:{bidder.LastName} came back from PayPal via cart GET");
            }
            else
            {
                return HttpNotFound();
            }

            var viewModel = GetBidderViewModel(bidder);
            return View(viewModel);
        }


        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult MarkBidderPaid(int bidderId, int ppTransId)
        {
            var ppTrans = db.PayPalTransactions.Find(ppTransId);
            var bidder = db.Bidders.Find(bidderId);
            if (ppTrans == null || bidder == null) return HttpNotFound();

            var invoiceRepos = new InvoiceRepository(db);
            var regInvoice = invoiceRepos.GetRegistrationInvoiceForBidder(bidder);
            invoiceRepos.ApplyPaymentToInvoice(ppTrans, regInvoice);
            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for bidder " + bidder.BidderId + " manually via MarkBidderPaid");
            return RedirectToAction("Index", "LogFiles");
        }

        [Authorize(Roles = AuctionRoles.CanAdminUsers)]
        public ActionResult MarkBidderPaidManually(int bidderId)
        {
            var bidder = db.Bidders.Find(bidderId);
            if (bidder == null) return HttpNotFound();

            var invoiceRepos = new InvoiceRepository(db);
            var regInvoice = invoiceRepos.GetRegistrationInvoiceForBidder(bidder);
            var isPaidAlready = regInvoice.IsPaid;
            invoiceRepos.ApplyPotentialManualPayment(regInvoice, PaymentMethod.Cash, User.Identity.GetUserName());
            db.SaveChanges();

            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for bidder " + bidder.BidderId + " manually via MarkBidderPaidManually");

            if (!isPaidAlready)
            {
                EmailHelperFactory.Instance().SendInvoicePaymentConfirmation(regInvoice, true);
            }

            return RedirectToAction("Index", "Bidders");
        }

        // GET: Bidders/Edit/5
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            var viewModel = GetBidderViewModel(bidder);
            return View(viewModel);
        }

        // POST: Bidders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "UpdateBy,UpdateDate")] BidderViewModel bidderViewModel)
        {
            if (ModelState.IsValid)
            {
                var bidder = Mapper.Map<Bidder>(bidderViewModel);

                bidder.UpdateBy = User.Identity.GetUserName();
                bidder.UpdateDate = Utilities.GetEasternTimeNow();

                foreach (var g in bidder.AuctionGuests)
                {
                    db.Entry(g).State = EntityState.Modified;
                }
                foreach (var s in bidder.Students)
                {
                    db.Entry(s).State = EntityState.Modified;
                }

                db.Entry(bidder).State = EntityState.Modified;

                //don't try to save StoreItemPurchases - just reload from DB
                //TODO: test this flow with bidder invoice refactor

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }

                return RedirectToAction("Index");
            }
            return View(bidderViewModel);
        }

        // GET: Bidders/Delete/5
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            var viewModel = GetBidderViewModel(bidder);
            return View(viewModel);
        }

        // POST: Bidders/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Bidder bidder = db.Bidders.Find(id);
            bidder.IsDeleted = true;

            var invoiceRepo = new InvoiceRepository(db);
            var regInvoice = invoiceRepo.GetRegistrationInvoiceForBidder(bidder);
            if (regInvoice != null)
            {
                invoiceRepo.DeleteInvoice(regInvoice);
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("SubmitBiddersAction")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult SubmitBiddersAction(string biddersAction, string selectedBidderNumbers, string startingPaddleNumber)
        {
            var bidderNumbers = selectedBidderNumbers
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            switch (biddersAction)
            {
                case "RenumberBidderPaddles":
                    var countOfBidders = db.Bidders.Count();
                    var maxPaddleNumberNow = db.Bidders.Max(b => b.BidderNumber);
                    var startingPaddleNumberInt = int.Parse(startingPaddleNumber);
                    var startOfUpperSet = Math.Max(startingPaddleNumberInt + countOfBidders, maxPaddleNumberNow) + 1;

                    var currentBidNum = startOfUpperSet;
                    //set all of them up high first
                    foreach (var bidder in db.Bidders)
                    {
                        bidder.BidderNumber = currentBidNum;
                        currentBidNum++;
                    }
                    db.SaveChanges();

                    //set them to the new values now
                    currentBidNum = startingPaddleNumberInt;
                    foreach (var bidder in db.Bidders.OrderBy(b => b.LastName).ThenBy(b => b.FirstName))
                    {
                        bidder.BidderNumber = currentBidNum;
                        currentBidNum++;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                case "RenumberSinglePaddle":
                    var paddleNumberInt = int.Parse(startingPaddleNumber); //passed via the same param as the renumber all
                    var existingBiddersUsingNumber = db.Bidders.Count(b => b.BidderNumber == paddleNumberInt);

                    if (existingBiddersUsingNumber > 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);                    
                    if (bidderNumbers.Count != 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var bidderNum = bidderNumbers.Single();
                    var bidderToUpdate = db.Bidders.Single(ai => ai.BidderNumber == bidderNum);
                    bidderToUpdate.BidderNumber = paddleNumberInt;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                case "SendBidderRegistrationNudge":
                    if (!bidderNumbers.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var selectedBidders = db.Bidders.Where(ai => bidderNumbers.Contains(ai.BidderNumber)).ToList();
                    if (!selectedBidders.Any()) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    var emailHelper = EmailHelperFactory.Instance();
                    var bidRepos = new BidderRepository(db);
                    foreach (var bidder in selectedBidders)
                    {
                        var hasPaid = bidRepos.HasBidderPaidForRegistration(bidder);
                        emailHelper.SendBidderRegistrationNudge(bidder, hasPaid);
                        bidder.IsCatalogNudgeEmailSent = true;
                    }
                    db.SaveChanges();

                    return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult MarkAsAttended(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bidder bidder = db.Bidders.Find(id);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            bidder.AttendedEvent = true;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult Login(string returnURl)
        {
            return View(new BidderLoginModel { RedirectUrl = returnURl });
        }

        // POST: AuctionItems/Delete/5
        [HttpPost, ActionName("Login")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LoginConfirmed(BidderLoginModel bidderLookup)
        {
            if (ModelState.IsValid)
            {
                if (!bidderLookup.BidderNumber.HasValue)
                {
                    var bidder = db.Bidders.FirstOrDefault(b =>
                        b.IsDeleted == false &&
                        b.LastName.Equals(bidderLookup.LastName, StringComparison.OrdinalIgnoreCase) &&
                        b.Email.Equals(bidderLookup.Email, StringComparison.OrdinalIgnoreCase));

                    if (bidder == null)
                    {
                        ModelState.AddModelError("", "No bidder was found matching this information.");
                    }
                    else
                    {
                        EmailHelperFactory.Instance().SendBidderRegistrationConfirmationOrPaddleReminder(bidder);
                        ModelState.AddModelError("", "Your bidder number has been emailed to you.  Please wait for the email then try again with your bidder number.");
                    }
                }
                else
                {
                    var bidder = db.Bidders.FirstOrDefault(b =>
                        b.IsDeleted == false &&
                        b.BidderNumber == bidderLookup.BidderNumber.Value &&
                        b.LastName.Equals(bidderLookup.LastName, StringComparison.OrdinalIgnoreCase) &&
                        b.Email.Equals(bidderLookup.Email, StringComparison.OrdinalIgnoreCase));

                    if (bidder == null)
                    {
                        ModelState.AddModelError("", "No bidder was found matching this information.");
                    }
                    else if (!new BidderRepository(db).IsBidderAllowedToBid(bidder))
                    {
                        ModelState.AddModelError("", "You must have a paid ticket in order to proceed to the mobile bidding site.");
                    }
                    else
                    {
                        BidderRepository.SetBidderCookie(bidder);

                        if (!string.IsNullOrEmpty(bidderLookup.RedirectUrl))
                        {
                            return Redirect(bidderLookup.RedirectUrl);
                        }

                        return RedirectToAction("Bids");
                    }
                }
            }

            return View(bidderLookup);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            BidderRepository.ClearBidderCookie();
            return RedirectToAction("Login", new { returnURl = "/bids" });
        }

        [AllowAnonymous]
        public ActionResult Bids(BidErrorType? error)
        {
            BidderCookieInfo info;
            if (BidderRepository.TryGetBidderInfoFromCookie(out info))
            {
                var bids = db.Bids
                    .Include(b => b.AuctionItem)
                    .Where(b => b.Bidder.BidderId == info.BidderId && b.Bidder.BidderNumber == info.BidderNumber)
                    .ToList();

                var bidsToShow = FilterOutMyExtraLosingBids(bids);
                var models = bidsToShow.Select(b => new BidViewModel(b, b.AuctionItem)).ToList();
                var itemFavs = new BidderRepository(db).GetBidderCatalogFavorites(info.BidderId, info.BidderNumber);

                var closeDate = new ItemsRepository(db).GetBiddingEndDateIfCategoriesAreOpen();
                ViewBag.IsBiddingOpen = closeDate.HasValue;
                if (closeDate.HasValue)
                {
                    ViewBag.BiddingEndDate = closeDate.Value.Subtract(Utilities.GetEasternTimeNow()).ToString("hh'h 'mm'm'");
                }

                var openDate = new ItemsRepository(db).GetBiddingStartDateIfCategoriesAreClosed();
                var showOpenDate = closeDate.HasValue == false &&
                    openDate.HasValue &&
                    openDate.Value > Utilities.GetEasternTimeNow() &&
                    openDate.Value.Subtract(Utilities.GetEasternTimeNow()).TotalDays < 30;
                ViewBag.ShowOpeningCountDown = showOpenDate;
                if (showOpenDate)
                {
                    ViewBag.BiddingStartDate = openDate.Value.Subtract(Utilities.GetEasternTimeNow()).ToString("dd'd 'hh'h 'mm'm'");
                }

                ViewBag.BidderInfo = info;

                if (error.HasValue)
                {
                    switch (error.Value)
                    {
                        case BidErrorType.InvalidItemNumber:
                            ModelState.AddModelError("", "You entered an invalid item number.  Please double-check the item number and try again.");
                            break;
                        default:
                            break;
                    }
                }
                return View(new ActiveBidsViewModel(models, itemFavs));
            }
            else
            {
                return RedirectToAction("Login", new { returnURl = "/bids" });
            }
        }

        private List<Bid> FilterOutMyExtraLosingBids(List<Bid> bids)
        {
            var bidsToReturn = new List<Bid>();
            var bidsByItem = bids.GroupBy(b => b.AuctionItem);
            foreach (var group in bidsByItem)
            {
                //for each item, only display my top bid or any that are winning (if winning multiples)
                var myMaxBidOnThisItem = group.OrderByDescending(b => b.BidAmount).FirstOrDefault();
                var bidsToInclude = group.Where(b => b.IsWinning || b == myMaxBidOnThisItem);
                bidsToReturn.AddRange(bidsToInclude);
            }
            return bidsToReturn;
        }

        [AllowAnonymous]
        public ActionResult UpdateCatalogFavorite(int auctionItemId)
        {
            Bidder bidder;
            if (new BidderRepository(db).TryGetValidatedBidderFromCookie(out bidder))
            {
                var item = db.AuctionItems.Find(auctionItemId);
                if (item == null)
                {
                    return Json(new { success = false, error = "Item not found" }, JsonRequestBehavior.AllowGet);
                }

                var existingFavorite = db.Bidders
                    .Where(b => b.BidderId == bidder.BidderId && b.BidderNumber == bidder.BidderNumber)
                    .SelectMany(b => b.CatalogFavorites)
                    .FirstOrDefault(f => f.AuctionItem.AuctionItemId == auctionItemId);

                //if it doesn't exist then create it otherwise remove it (basically just flip it)
                if (existingFavorite == null)
                {
                    var fav = new CatalogFavorite { AuctionItem = item };
                    bidder.CatalogFavorites.Add(fav);
                    db.SaveChanges();
                }
                else
                {
                    db.CatalogFavorites.Remove(existingFavorite);
                    db.SaveChanges();
                }

                return Json(new { success = true, error = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, error = "You must log in first" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult EnterBid(string itemNo)
        {
            BidderCookieInfo info;
            if (BidderRepository.TryGetBidderInfoFromCookie(out info))
            {
                ViewBag.BidderInfo = info;

                AuctionItem item = null;
                int itemNoInt;
                if (int.TryParse(itemNo, out itemNoInt))
                {
                    item = new ItemsRepository(db).GetItemWithAllBidInfo(itemNoInt);
                }
                if (item == null)
                {
                    return RedirectToAction("Bids", new { error = BidErrorType.InvalidItemNumber });
                }
                var nextBidIncrement = !item.IsFixedPrice && !item.IsInFiresale && item.AllBids.Any() ?
                    item.AllBids.Max(b => b.BidAmount) + item.BidIncrement :
                    item.StartingBid;

                var model = GetBidEnterModel(item, nextBidIncrement);
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", new { returnURl = "/bids" });
            }
        }

        [HttpPost, ActionName("EnterBid")]
        [AllowAnonymous]
        public ActionResult EnterBidConfirmed(int itemNo, decimal? bidAmount)
        {
            Bidder bidder;
            if (new BidderRepository(db).TryGetValidatedBidderFromCookie(out bidder))
            {
                ViewBag.BidderInfo = bidder;

                var item = new ItemsRepository(db).GetItemWithAllBidInfo(itemNo);
                if (item == null)
                {
                    return HttpNotFound();
                }

                var canBid = item.Category.IsBiddingOpen || item.IsInFiresale || (Request.IsAuthenticated && User.IsInRole(GlobeAuction.Models.AuctionRoles.CanAdminUsers));
                var highestExistingBid = item.AllBids.Select(b => b.BidAmount).DefaultIfEmpty(0).Max();
                var bidAmountValue = bidAmount.GetValueOrDefault(99999);

                if (!bidAmount.HasValue)
                {
                    ModelState.AddModelError("", "You must enter a bid amount.");
                }
                else if (item.StartingBid > bidAmountValue)
                {
                    ModelState.AddModelError("", "Your bid must be equal to or higher than the starting bid (" + item.StartingBid.ToString("c") + ").  Please increase your bid and try again.");
                }
                else if ((item.IsFixedPrice || item.IsInFiresale) && item.StartingBid != bidAmountValue)
                {
                    ModelState.AddModelError("", "Your bid must be equal to the fixed price (" + item.StartingBid.ToString("c") + ").  Please correct your bid and try again.");
                }
                else if (!item.IsFixedPrice && (bidAmountValue - item.StartingBid) % item.BidIncrement != 0)
                {
                    ModelState.AddModelError("", "Your bid must be an increment of the Bid Increment (" + item.BidIncrement.ToString("c") + ").  Please adjust your bid and try again.");
                }
                else if (bidAmountValue <= highestExistingBid && !item.IsFixedPrice && !item.IsInFiresale)
                {
                    ModelState.AddModelError("", "Your bid must be higher than the last bid of " + highestExistingBid.ToString("c") + ".  You need to increase your bid and place it again.");
                }
                else if ((item.IsFixedPrice || item.IsInFiresale) && item.AllBids.Count >= item.Quantity)
                {
                    ModelState.AddModelError("", $"All {item.Quantity} quantity available are already purchased for this item.");
                }
                else if (!canBid)
                {
                    ModelState.AddModelError("", "Bidding is currently closed for this category.");
                }
                else
                {
                    var lockKey = "BidLock:" + itemNo;
                    using (var lockForBidding = MutexByKey.TryGetLock(lockKey))
                    {
                        if (lockForBidding.WasAcquired)
                        {
                            //enter the bid and return success on the Bids list page
                            List<Bidder> biddersThatLost;
                            new ItemsRepository(db).EnterNewBidAndRecalcWinners(item, bidder, bidAmountValue, out biddersThatLost);

                            //after saving DB changes, now go text those bidders
                            var bidLink = Url.Action("EnterBid", "Bidders", new { itemNo = item.UniqueItemNumber }, Request.Url.Scheme);
                            //var body = string.Format("You have been outbid on item # {0} ({1}).  Click here to rebid: {2}", itemNo, item.Title, bidLink);

                            //var txtHelper = new SmsHelper();
                            var emailHelper = new EmailHelper();
                            foreach (var lostBidder in biddersThatLost)
                            {
                                //txtHelper.SendSms(lostBidder.Phone, body);
                                emailHelper.SendBidderOutbidEmail(lostBidder, item.UniqueItemNumber, item.Title, bidLink);
                            }

                            return RedirectToAction("Bids", "Bidders");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Someone else is entering a bid at this exact time - please try again.");
                        }
                    }
                }

                var model = GetBidEnterModel(item, bidAmountValue);
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", new { returnURl = "/bids" });
            }
        }

        private EnterBidViewModel GetBidEnterModel(AuctionItem item, decimal bidAmount)
        {
            return new EnterBidViewModel
            {
                AuctionItem = new AuctionItemViewModel(item),
                AllBids = item.AllBids.Select(b => new BidViewModel(b, item)).ToList(),
                BidAmount = bidAmount,
                IsBiddingOpen = item.Category.IsBiddingOpen
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddBidderControlInfo(string promoCode)
        {
            //TICKETS
            ViewBag.TicketTypes = GetTicketTypesForRegistration(promoCode)
                .OrderByDescending(t => t.Price)
                .Select(t => new SelectListItem { Text = string.Format("{0} - {1:C}", t.Name, t.Price), Value = t.TicketTypeId.ToString() }).ToList();


            //TEACHER NAMES
            ViewBag.TeacherNames = ConfigHelper.GetTeacherNames()
                .Select(t => new SelectListItem { Text = t, Value = t });
        }

        private List<TicketType> GetTicketTypesForRegistration(string promoCode)
        {
            var ticketTypes = db.TicketTypes.ToList();

            var userIsAdmin = Request.IsAuthenticated && User.IsInRole(AuctionRoles.CanAdminUsers);

            //remove admin-only ticket types if not an admin
            ticketTypes = ticketTypes.Where(t => userIsAdmin || t.OnlyVisibleToAdmins == false).ToList();

            //remove promo code tickets unless they have entered the promo code
            ticketTypes = ticketTypes.Where(t => userIsAdmin || string.IsNullOrEmpty(t.PromoCode) || t.PromoCode.Equals(promoCode, StringComparison.OrdinalIgnoreCase)).ToList();

            return ticketTypes;
        }

        private BidderViewModel GetBidderViewModel(Bidder bidder)
        {
            var viewModel = Mapper.Map<BidderViewModel>(bidder);
            viewModel.RegistrationInvoice = db.Invoices.FirstOrDefault(i => i.InvoiceType == InvoiceType.BidderRegistration && i.Bidder.BidderId == bidder.BidderId);
            return viewModel;
        }

        [HttpGet]
        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult SendSms(string phone, string msg)
        {
            new SmsHelper().SendSms(phone, msg);
            return RedirectToAction("Index");
        }
    }
}
