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
            var bidders = db.Bidders
                .Include(b => b.AuctionGuests)
                .Include(b => b.StoreItemPurchases)
                .Where(b => b.IsDeleted == false).ToList();

            var models = bidders.Select(b => new BidderForList(b)).ToList();
            
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

            var viewModel = Mapper.Map<BidderViewModel>(bidder);
            return View(viewModel);
        }

        // GET: Bidders/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var storeItems = db.StoreItems.Where(s => s.CanPurchaseInBidderRegistration && s.IsDeleted == false).ToList();
            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanEditBidders))
            {
                //remove admin-only ticket types
                storeItems = storeItems.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }
            var availableStoreItems = storeItems.Select(i => Mapper.Map<StoreItemViewModel>(i)).ToList();

            AddBidderControlInfo();
            var newBidder = new BidderViewModel()
            {
                AuctionGuests = new List<AuctionGuestViewModel>(Enumerable.Repeat(new AuctionGuestViewModel(), 6)),
                Students = new List<StudentViewModel>(Enumerable.Repeat(new StudentViewModel(), 4)),
                StoreItemPurchases = availableStoreItems.Select(si => new StoreItemPurchaseViewModel
                {
                    StoreItem = si
                }).ToList()
            };

            return View(newBidder);
        }

        // POST: Bidders/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "BidderId")] BidderViewModel bidderViewModel, string submitButton)
        {
            if (ModelState.IsValid)
            {
                var bidder = Mapper.Map<Bidder>(bidderViewModel);

                bidder.CreateDate = bidder.UpdateDate = DateTime.Now;
                bidder.UpdateBy = bidder.Email;

                //strip out dependents that weren't filled in
                bidder.Students = bidderViewModel.Students.Where(s => !string.IsNullOrEmpty(s.HomeroomTeacher)).Select(s => Mapper.Map<Student>(s)).ToList();
                bidder.AuctionGuests = bidderViewModel.AuctionGuests.Where(g => !string.IsNullOrEmpty(g.FirstName)).Select(s => Mapper.Map<AuctionGuest>(s)).ToList();
                
                bidder.StoreItemPurchases = (bidderViewModel.StoreItemPurchases ?? new List<StoreItemPurchaseViewModel>())
                    .Where(s => s.Quantity > 0)
                    .Select(s => Mapper.Map<StoreItemPurchase>(s))
                    .ToList();

                foreach(var sip in bidder.StoreItemPurchases)
                {
                    db.Entry(sip.StoreItem).State = EntityState.Unchanged;
                }

                foreach (var guest in bidder.AuctionGuests)
                {
                    var ticketType = db.TicketTypes.Find(int.Parse(guest.TicketType));
                    guest.TicketType = ticketType.Name;
                    guest.TicketPrice = ticketType.Price;
                }
                
                db.Bidders.Add(bidder);
                db.SaveChanges();

                if (submitButton == "Create Bidder Only")
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("RedirectToPayPal", new { id = bidder.BidderId });
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

            var viewModel = new BidderForPayPal(bidder);
            
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

            new BidderRepository(db).ApplyTicketPaymentToBidder(ppTrans, bidder);
            NLog.LogManager.GetCurrentClassLogger().Info("Updated payment for bidder " + bidder.BidderId + " via cart post-back");

            var viewModel = Mapper.Map<BidderViewModel>(bidder);
            return View(viewModel);
        }


        [Authorize(Roles = AuctionRoles.CanEditBidders)]
        public ActionResult MarkBidderPaid(int bidderId, int ppTransId)
        {
            var ppTrans = db.PayPalTransactions.Find(ppTransId);
            var bidder = db.Bidders.Find(bidderId);
            if (ppTrans == null || bidder == null) return HttpNotFound();
            
            new BidderRepository(db).ApplyTicketPaymentToBidder(ppTrans, bidder);
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

            var viewModel = Mapper.Map<BidderViewModel>(bidder);
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
                bidder.UpdateDate = DateTime.Now;
                
                foreach(var g in bidder.AuctionGuests)
                {
                    db.Entry(g).State = EntityState.Modified;
                }
                foreach (var s in bidder.Students)
                {
                    db.Entry(s).State = EntityState.Modified;
                }

                db.Entry(bidder).State = EntityState.Modified;

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

            var viewModel = Mapper.Map<BidderViewModel>(bidder);
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

            if (!Request.IsAuthenticated || !User.IsInRole(AuctionRoles.CanEditBidders))
            {
                //remove admin-only ticket types
                ticketTypes = ticketTypes.Where(t => t.OnlyVisibleToAdmins == false).ToList();
            }

            ViewBag.TicketTypes = ticketTypes
                .Select(t => new SelectListItem { Text = string.Format("{0} - {1:C}", t.Name, t.Price), Value = t.TicketTypeId.ToString() }).ToList();

            
            //TEACHER NAMES
            ViewBag.TeacherNames = AuctionConstants.TeacherNames
                .Select(t => new SelectListItem { Text = t, Value = t });
        }
    }
}
