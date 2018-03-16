using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GlobeAuction.Models
{
    public class AllRevenueByTypeReportModel
    {
        [Display(Name = "Event Tickets")]
        public decimal BidderTickets { get; set; }

        [Display(Name = "Raffle Tickets Purchased in Registration")]
        public decimal RaffleTicketsViaRegistration { get; set; }

        [Display(Name = "Raffle Tickets Purchased in Store")]
        public decimal RaffleTicketsViaStore { get; set; }

        [Display(Name = "Store Sales Purchased in Registration")]
        public decimal StoreSalesViaRegistration { get; set; }

        [Display(Name = "Store Sales Purchased in Store or Checkout")]
        public decimal StoreSalesViaStore { get; set; }
        
        [Display(Name = "Auction Items")]
        public decimal AuctionItems { get; set; }

        [Display(Name = "Total Raised")]
        public decimal Total
        {
            get { return BidderTickets + RaffleTicketsViaRegistration + RaffleTicketsViaStore + StoreSalesViaRegistration + StoreSalesViaStore + AuctionItems; }
        }
    }

    public class FundaProjectRevenueReportModel
    {
        [Display(Name = "Purchased in Store or Checkout (unpaid")]
        public decimal SalesViaStoreUnpaid { get; set; }

        [Display(Name = "Purchased in Live Auction (unpaid")]
        public decimal SalesViaAuctionUnpaid { get; set; }

        [Display(Name = "Purchased in Store or Checkout (paid")]
        public decimal SalesViaStorePaid { get; set; }

        [Display(Name = "Purchased in Live Auction (paid")]
        public decimal SalesViaAuctionPaid { get; set; }
        
        [Display(Name = "Total Raised")]
        public decimal Total
        {
            get { return SalesViaStoreUnpaid + SalesViaAuctionUnpaid + SalesViaStorePaid + SalesViaAuctionPaid; }
        }
    }

    public class RaffleTicketPurchasesReportModel
    {
        public List<RaffleTicketPurchaseGroup> PaidRaffleTickets { get; set; }

        [Display(Name = "Total Raised")]
        public decimal Total
        {
            get { return PaidRaffleTickets.Select(t => t.TotalSales).DefaultIfEmpty(0).Sum(); }
        }
    }

    public class AllRevenueReportsModel
    {
        public AllRevenueByTypeReportModel AllRevenueByTypeReport { get; set; }
        public FundaProjectRevenueReportModel FundaProjectRevenueReport { get; set; }
        public RaffleTicketPurchasesReportModel RaffleTicketPurchasesReport { get; set; }
    }

    public class RaffleTicketPurchaseGroup
    {
        public string RaffleTicketName { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalSales { get; set; }
    }
}