using GlobeAuction.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace GlobeAuction.Helpers
{
    public class RevenueHelper
    {
        private ApplicationDbContext db;

        private static decimal _totalRevenue = 0m;
        private static object _revenueLock = new object();

        public static void SetTotalRevenue(decimal rev)
        {
            lock (_revenueLock)
            {
                _totalRevenue = rev;
            }
        }

        public static void IncrementTotalRevenue(decimal rev)
        {
            lock (_revenueLock)
            {
                _totalRevenue += rev;
            }
        }

        public static decimal GetTotalRevenue()
        {
            return _totalRevenue;
        }


        public RevenueHelper(ApplicationDbContext context)
        {
            db = context;
        }


        public AllRevenueByTypeReportModel GetAllRevenueByType()
        {
            List<Invoice> allInvoices;
            List<Invoice> paidInvoices;
            List<Bid> unpaidBids;
            List<Bid> paidBids;
            return GetAllRevenueByType(out allInvoices, out paidInvoices, out unpaidBids, out paidBids);
        }

        public AllRevenueByTypeReportModel GetAllRevenueByType(out List<Invoice> allInvoices, out List<Invoice> paidInvoices, out List<Bid> unpaidBids, out List<Bid> paidBids)
        {
            allInvoices = db.Invoices
                .Include(i => i.StoreItemPurchases)
                .Include(i => i.TicketPurchases)
                .Include("StoreItemPurchases.StoreItem")
                .ToList();
            paidInvoices = allInvoices.Where(i => i.IsPaid).ToList();
            var paidCheckoutInvoices = paidInvoices.Where(i => i.InvoiceType == InvoiceType.AuctionCheckout).ToList();
            var paidRegistrationInvoices = paidInvoices.Where(i => i.InvoiceType == InvoiceType.BidderRegistration).ToList();

            unpaidBids = db.Bids
                .Include(b => b.AuctionItem.Category)
                .Where(b => b.IsWinning && (b.Invoice == null || b.Invoice.IsPaid == false))
                .ToList();
            var unpaidBidsAmount = unpaidBids.Select(b => b.BidAmount).DefaultIfEmpty(0).Sum();

            paidBids = db.Bids
                .Include(b => b.AuctionItem.Category)
                .Where(b => b.IsWinning && b.AmountPaid.HasValue)
                .ToList();

            var paidBidsAmount = paidBids
                .Select(b => b.AmountPaid.Value)
                .DefaultIfEmpty(0)
                .Sum();

            return new AllRevenueByTypeReportModel
            {
                AuctionItemsPaid = paidBidsAmount,
                AuctionItemsUnpaid = unpaidBidsAmount,
                BidderTickets = paidRegistrationInvoices.Sum(i => i.TicketPurchases.Sum(t => t.TicketPricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaRegistration = paidRegistrationInvoices.Sum(b => b.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaRegistration = paidRegistrationInvoices.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaStore = paidCheckoutInvoices.Sum(i => i.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaStore = paidCheckoutInvoices.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
            };
        }
    }
}