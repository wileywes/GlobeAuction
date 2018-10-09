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
                .Include(i => i.StoreItemPurchases)
                .Include(i => i.TicketPurchases)
                .Include("StoreItemPurchases.StoreItem")
                .ToList();
            
            var paidInvoices = allInvoices.Where(i => i.IsPaid).ToList();

            var paidCheckoutInvoices = paidInvoices.Where(i => i.InvoiceType == InvoiceType.AuctionCheckout).ToList();
            var paidRegistrationInvoices = paidInvoices.Where(i => i.InvoiceType == InvoiceType.BidderRegistration).ToList();

            var unpaidBids = db.Bids
                .Where(b => b.IsWinning && (b.Invoice == null || b.Invoice.IsPaid == false))
                .Select(b => b.BidAmount)
                .DefaultIfEmpty(0)
                .Sum();
            
            var paidBids = db.Bids
                .Where(b => b.IsWinning && b.AmountPaid.HasValue)
                .Select(b => b.AmountPaid.Value)
                .DefaultIfEmpty(0)
                .Sum();

            var byType = new AllRevenueByTypeReportModel
            {
                AuctionItemsPaid = paidBids,
                AuctionItemsUnpaid = unpaidBids,
                BidderTickets = paidRegistrationInvoices.Sum(i => i.TicketPurchases.Sum(t => t.TicketPricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaRegistration = paidRegistrationInvoices.Sum(b => b.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaRegistration = paidRegistrationInvoices.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                RaffleTicketsViaStore = paidCheckoutInvoices.Sum(i => i.StoreItemPurchases.Where(sip => sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
                StoreSalesViaStore = paidCheckoutInvoices.Sum(b => b.StoreItemPurchases.Where(sip => !sip.StoreItem.IsRaffleTicket).Sum(sip => sip.PricePaid.GetValueOrDefault(0))),
            };

            //fund-a-project
            var fundProjectBids = db.Bids
                .Include(i => i.Invoice)
                .Where(b => b.AuctionItem.Category == AuctionConstants.FundaProjectCategoryName)
                .ToList();

            var fundAProject = new FundaProjectRevenueReportModel
            {
                SalesViaAuctionPaid = fundProjectBids.Where(b => b.IsWinning && b.AmountPaid.HasValue).Select(b => b.AmountPaid.Value).DefaultIfEmpty(0).Sum(),
                SalesViaAuctionUnpaid = fundProjectBids.Where(b => b.IsWinning && b.AmountPaid.HasValue == false).Select(b => b.BidAmount).DefaultIfEmpty(0).Sum(),
                SalesViaStorePaid = allInvoices.Where(i => i.IsPaid).SelectMany(i => i.StoreItemPurchases).Where(sip => sip.IsPaid && !sip.StoreItem.IsRaffleTicket && sip.StoreItem.HasUnlimitedQuantity).Select(sip => sip.PricePaid.GetValueOrDefault(0)).DefaultIfEmpty(0).Sum(),
                SalesViaStoreUnpaid = allInvoices.Where(i => !i.IsPaid).SelectMany(i => i.StoreItemPurchases).Where(sip => !sip.IsPaid && !sip.StoreItem.IsRaffleTicket && sip.StoreItem.HasUnlimitedQuantity).Select(sip => sip.Price).DefaultIfEmpty(0).Sum()
            };

            //raffle tickets are already broken down into individual records when bundles are purchases, so just need to sum by store item title
            var paidRafflePurchases = paidInvoices.SelectMany(i => i.StoreItemPurchases).Where(sip => sip.IsPaid && sip.StoreItem.IsRaffleTicket).ToList();

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
