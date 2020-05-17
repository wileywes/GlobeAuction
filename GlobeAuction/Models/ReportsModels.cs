using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GlobeAuction.Models
{
    public class AllRevenueByTypeReportModel
    {
        [Display(Name = "Event Tickets")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal BidderTickets { get; set; }

        [Display(Name = "Raffle Tickets Purchased in Registration")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal RaffleTicketsViaRegistration { get; set; }

        [Display(Name = "Raffle Tickets Purchased in Store")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal RaffleTicketsViaStore { get; set; }

        [Display(Name = "Store Sales Purchased in Registration")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal StoreSalesViaRegistration { get; set; }

        [Display(Name = "Store Sales Purchased in Store or Checkout")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal StoreSalesViaStore { get; set; }
        
        [Display(Name = "Auction Items (paid)")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal AuctionItemsPaid { get; set; }

        [Display(Name = "Auction Items (unpaid)")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal AuctionItemsUnpaid { get; set; }

        [Display(Name = "Total Raised")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal Total
        {
            get { return BidderTickets + RaffleTicketsViaRegistration + RaffleTicketsViaStore + StoreSalesViaRegistration + StoreSalesViaStore + AuctionItemsPaid + AuctionItemsUnpaid; }
        }
    }

    public class FundaProjectRevenueReportModel
    {
        [Display(Name = "Purchased in Store or Checkout (unpaid)")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal SalesViaStoreUnpaid { get; set; }

        [Display(Name = "Purchased in Live Auction (unpaid)")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal SalesViaAuctionUnpaid { get; set; }

        [Display(Name = "Purchased in Store or Checkout (paid)")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal SalesViaStorePaid { get; set; }

        [Display(Name = "Purchased in Live Auction (paid)")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal SalesViaAuctionPaid { get; set; }
        
        [Display(Name = "Total Raised")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal Total
        {
            get { return SalesViaStoreUnpaid + SalesViaAuctionUnpaid + SalesViaStorePaid + SalesViaAuctionPaid; }
        }
    }

    public class RaffleTicketPurchasesReportModel
    {
        public List<RaffleTicketPurchaseGroup> PaidRaffleTickets { get; set; }

        [Display(Name = "Total Raised")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal Total
        {
            get { return PaidRaffleTickets.Select(t => t.TotalSales).DefaultIfEmpty(0).Sum(); }
        }
    }

    public class PurchasesByPaymentMethodReportModel
    {
        public List<PurchasesByPaymentMethodGroup> PurchasesByPaymentMethod { get; set; }

        [Display(Name = "Total Raised")]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal Total
        {
            get { return PurchasesByPaymentMethod.Select(t => t.TotalSales).DefaultIfEmpty(0).Sum(); }
        }
    }

    public class AllRevenueReportsModel
    {
        public AllRevenueByTypeReportModel AllRevenueByTypeReport { get; set; }
        public FundaProjectRevenueReportModel FundaProjectRevenueReport { get; set; }
        public RaffleTicketPurchasesReportModel RaffleTicketPurchasesReport { get; set; }
        public PurchasesByPaymentMethodReportModel PurchasesByPaymentMethodReport { get; set; }
        public List<RevenueByAuctionCategory> RevenueByAuctionCategoryReport { get; set; }
    }

    public class RaffleTicketPurchaseGroup
    {
        public string RaffleTicketName { get; set; }
        public int TicketCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal TotalSales { get; set; }
    }

    public class PurchasesByPaymentMethodGroup
    {
        public string PaymentMethodName { get; set; }
        public int PurchaseCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal TotalSales { get; set; }
    }

    public class RevenueByAuctionCategory
    {
        public string CategoryName { get; set; }
        public decimal TotalWinningBids { get; set; }
    }
}