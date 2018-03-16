using AutoMapper;
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
    [Authorize(Roles = AuctionRoles.CanAdminUsers)]
    public class ReportsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult AllRevenueByType()
        {
            var paidInvoices = db.Invoices
                .Include(i => i.AuctionItems)
                .Include(i => i.StoreItemPurchases)
                .Include("StoreItemPurchases.StoreItem")
                .Where(i => i.IsPaid)
                .ToList();
            
            var allBidders = db.Bidders
                .Include(b => b.AuctionGuests)
                .Include(b => b.StoreItemPurchases)
                .Include("StoreItemPurchases.StoreItem")
                .ToList();

            var model = new AllRevenueByTypeReportModel
            {
                AuctionItems = paidInvoices.Sum(i => i.AuctionItems.Sum(a => a.WinningBid.GetValueOrDefault(0))),
                BidderTickets = allBidders.Sum(b => b.AuctionGuests.Sum(g => g.TicketPricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaRegistration = allBidders.Sum(b => b.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaRegistration = allBidders.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaStore = paidInvoices.Sum(i => i.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaStore = paidInvoices.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
            };

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
