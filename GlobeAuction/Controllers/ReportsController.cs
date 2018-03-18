using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using GlobeAuction.Models;

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanAdminUsers)]
    public class ReportsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult AllRevenueReports()
        {
            var allInvoices = db.Invoices
                .Include(i => i.AuctionItems)
                .Include(i => i.StoreItemPurchases)
                .Include("StoreItemPurchases.StoreItem")
                .ToList();
            
            var allBidders = db.Bidders
                .Include(b => b.AuctionGuests)
                .Include(b => b.StoreItemPurchases)
                .Include("StoreItemPurchases.StoreItem")
                .Where(b => b.IsDeleted == false)
                .ToList();

            var paidInvoices = allInvoices.Where(i => i.IsPaid).ToList();

            var unpaidAuctionItems = db.AuctionItems
                .Where(ai => ai.WinningBid.HasValue && (ai.Invoice == null || ai.Invoice.IsPaid == false))
                .Select(ai => ai.WinningBid.Value)
                .DefaultIfEmpty(0)
                .Sum();

            var byType = new AllRevenueByTypeReportModel
            {
                AuctionItemsPaid = paidInvoices.Sum(i => i.AuctionItems.Sum(a => a.WinningBid.GetValueOrDefault(0))),
                AuctionItemsUnpaid = unpaidAuctionItems,
                BidderTickets = allBidders.Sum(b => b.AuctionGuests.Sum(g => g.TicketPricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaRegistration = allBidders.Sum(b => b.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaRegistration = allBidders.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaStore = paidInvoices.Sum(i => i.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaStore = paidInvoices.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
            };

            //fund-a-project
            var fapAuctionItems = db.AuctionItems
                .Include(i => i.Invoice)
                .Where(ai => ai.Category == AuctionConstants.FundaProjectCategoryName)
                .ToList();

            var fundAProject = new FundaProjectRevenueReportModel
            {
                SalesViaAuctionPaid = fapAuctionItems.Where(ai => ai.Invoice != null && ai.Invoice.IsPaid && ai.WinningBid.HasValue).Select(ai => ai.WinningBid.Value).DefaultIfEmpty(0).Sum(),
                SalesViaAuctionUnpaid = fapAuctionItems.Where(ai => (ai.Invoice == null || !ai.Invoice.IsPaid) && ai.WinningBid.HasValue).Select(ai => ai.WinningBid.Value).DefaultIfEmpty(0).Sum(),
                SalesViaStorePaid = allInvoices.Where(i => i.IsPaid).SelectMany(i => i.StoreItemPurchases).Where(sip => sip.IsPaid && !sip.StoreItem.IsRaffleTicket && sip.StoreItem.HasUnlimitedQuantity).Select(sip => sip.PricePaid.GetValueOrDefault(0)).DefaultIfEmpty(0).Sum(),
                SalesViaStoreUnpaid = allInvoices.Where(i => !i.IsPaid).SelectMany(i => i.StoreItemPurchases).Where(sip => !sip.IsPaid && !sip.StoreItem.IsRaffleTicket && sip.StoreItem.HasUnlimitedQuantity).Select(sip => sip.Price).DefaultIfEmpty(0).Sum()
            };

            //raffle tickets are already broken down into individual records when bundles are purchases, so just need to sum by store item title
            var paidRafflePurchases = paidInvoices.SelectMany(i => i.StoreItemPurchases).Where(sip => sip.IsPaid && sip.StoreItem.IsRaffleTicket).ToList();
            paidRafflePurchases.AddRange(allBidders.SelectMany(b => b.StoreItemPurchases).Where(sip => sip.IsPaid && sip.StoreItem.IsRaffleTicket));

            var raffleTicketPurchases = new RaffleTicketPurchasesReportModel
            {
                PaidRaffleTickets = paidRafflePurchases.GroupBy(r => r.StoreItem.Title).Select(g =>
                {
                    var ticketTitle = g.Key;
                    var ticketsForGroup = g.ToList();
                    return new RaffleTicketPurchaseGroup
                    {
                        RaffleTicketName = ticketTitle,
                        TicketCount = ticketsForGroup.Count,
                        TotalSales = ticketsForGroup.Select(sip => sip.PricePaid.GetValueOrDefault(0)).DefaultIfEmpty(0m).Sum()
                    };
                }).ToList()
            };

            //funds by payment method
            var paidPurchases = paidInvoices.GroupBy(i => i.PaymentMethod).Select(g =>
            {
                var payMethod = g.Key;
                var ordersForMethod = g.ToList();
                return new PurchasesByPaymentMethodGroup
                {
                    PaymentMethodName = GetPayMethodName(payMethod),
                    PurchaseCount = ordersForMethod.Count,
                    TotalSales = ordersForMethod.Select(i => i.TotalPaid).DefaultIfEmpty(0).Sum()
                };
            }).ToList();
            foreach(var b in allBidders)
            {
                if (b.TotalPaid > 0)
                {
                    var name = GetPayMethodName(b.PaymentMethod);
                    var methodGroup = paidPurchases.FirstOrDefault(p => p.PaymentMethodName == name);
                    if (methodGroup == null)
                    {
                        methodGroup = new PurchasesByPaymentMethodGroup();
                        methodGroup.PaymentMethodName = name;
                        paidPurchases.Add(methodGroup);
                    }
                    methodGroup.PurchaseCount += 1;
                    methodGroup.TotalSales += b.TotalPaid;
                }
            }
            var byPayMethodReport = new PurchasesByPaymentMethodReportModel
            {
                PurchasesByPaymentMethod = paidPurchases
            };

            var model = new AllRevenueReportsModel
            {
                AllRevenueByTypeReport = byType,
                FundaProjectRevenueReport = fundAProject,
                RaffleTicketPurchasesReport = raffleTicketPurchases,
                PurchasesByPaymentMethodReport = byPayMethodReport
            };
            return View(model);
        }

        private static string GetPayMethodName(PaymentMethod? payMethod)
        {
            return payMethod.HasValue ? payMethod.Value.ToString() : "Unknown";
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
