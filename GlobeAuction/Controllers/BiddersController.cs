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
using System.Configuration;
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

            var models = new List<BidderForList>();

            foreach(var bidder in bidders)
            {
                var invoice = invoicesForBidders.FirstOrDefault(i => i.Bidder.BidderId == bidder.BidderId);
                var bidderForList = new BidderForList(bidder, invoice);
                models.Add(bidderForList);
            }
            
            return View(models);
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
        public ActionResult Register(int? bid, string bem)
        {
            var raffleItems = db.StoreItems.Where(s => s.CanPurchaseInBidderRegistration && s.IsDeleted == false && s.IsRaffleTicket).ToList();
            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanAdminUsers))
            {
                //remove admin-only ticket types
                raffleItems = raffleItems.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }

            AddBidderControlInfo();
            var newBidder = new BidderRegistrationViewModel()
            {
                AuctionGuests = new List<AuctionGuestViewModel>(Enumerable.Repeat(new AuctionGuestViewModel(), 6)),
                Students = new List<StudentViewModel>(Enumerable.Repeat(new StudentViewModel(), 4)),
                ItemPurchases = raffleItems.Select(si => new BuyItemViewModel(si)).ToList()
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
        public ActionResult Register([Bind(Exclude = "BidderId")] BidderRegistrationViewModel bidderViewModel, string submitButton)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var bidder = Mapper.Map<Bidder>(bidderViewModel);
                    var updatedBy = User.Identity.GetUserName();
                    if (string.IsNullOrEmpty(updatedBy)) updatedBy = bidder.Email;

                    var nextBidderNumber = db.Bidders.Select(b => b.BidderNumber).DefaultIfEmpty(Constants.StartingBidderNumber - 1).Max() + 1;
                    bidder.BidderNumber = nextBidderNumber;
                    bidder.CreateDate = bidder.UpdateDate = Utilities.GetEasternTimeNow();
                    bidder.UpdateBy = updatedBy;

                    //strip out dependents that weren't filled in
                    bidder.Students = bidderViewModel.Students.Where(s => !string.IsNullOrEmpty(s.HomeroomTeacher)).Select(s => Mapper.Map<Student>(s)).ToList();
                    bidder.AuctionGuests = bidderViewModel.AuctionGuests.Where(g => !string.IsNullOrEmpty(g.FirstName)).Select(s => Mapper.Map<AuctionGuest>(s)).ToList();
                    
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

                    new InvoiceRepository(db).CreateInvoiceForBidderRegistration(bidder, bidderViewModel, manualPayMethod, updatedBy);

                    if (manualPayMethod.HasValue)
                    {
                        return RedirectToAction("Register", new { bid = bidder.BidderId, bem = bidder.Email });
                    }
                    else
                    {
                        return RedirectToAction("RedirectToPayPal", new { id = bidder.BidderId });
                    }
                }
                catch(OutOfStockException oosExc)
                {
                    ModelState.AddModelError("", $"Item \"{oosExc.StoreItem.Title}\" is no longer available.  Please refresh this page and try your registration again (you have not been charged yet).");
                }
            }

            AddBidderControlInfo();
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
            foreach(var guest in bidder.AuctionGuests)
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
        [HttpPost]
        public ActionResult PayPalComplete(FormCollection form)
        {
            var ppTrans = new PayPalTransaction(form);
            db.PayPalTransactions.Add(ppTrans);
            db.SaveChanges(); //go ahead and record the transaction

            var bidderId = BidderRepository.GetBidderIdFromTransaction(ppTrans);
            if (!bidderId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Bidder bidder = db.Bidders.Find(bidderId.Value);
            if (bidder == null)
            {
                return HttpNotFound();
            }

            var invoiceRepos = new InvoiceRepository(db);
            var regInvoice = invoiceRepos.GetRegistrationInvoiceForBidder(bidder);
            invoiceRepos.ApplyPaymentToInvoice(ppTrans, regInvoice);
            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for bidder " + bidder.BidderId + " via cart post-back");

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
            return RedirectToAction("Index", "Logs");
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
                
                foreach(var g in bidder.AuctionGuests)
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
        public ActionResult SubmitBiddersAction(string biddersAction, string startingPaddleNumber)
        {
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddBidderControlInfo()
        {
            //TICKETS
            var ticketTypes = db.TicketTypes.ToList();

            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanAdminUsers))
            {
                //remove admin-only ticket types
                ticketTypes = ticketTypes.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }

            ViewBag.TicketTypes = ticketTypes
                .OrderByDescending(t => t.Price)
                .Select(t => new SelectListItem { Text = string.Format("{0} - {1:C}", t.Name, t.Price), Value = t.TicketTypeId.ToString() }).ToList();

            
            //TEACHER NAMES
            ViewBag.TeacherNames = AuctionConstants.TeacherNames
                .Select(t => new SelectListItem { Text = t, Value = t });
        }

        private BidderViewModel GetBidderViewModel(Bidder bidder)
        {
            var viewModel = Mapper.Map<BidderViewModel>(bidder);
            viewModel.RegistrationInvoice = db.Invoices.FirstOrDefault(i => i.InvoiceType == InvoiceType.BidderRegistration && i.Bidder.BidderId == bidder.BidderId);
            return viewModel;
        }
    }
}
