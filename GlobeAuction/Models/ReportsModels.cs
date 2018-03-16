using System.ComponentModel.DataAnnotations;

namespace GlobeAuction.Models
{
    public class AllRevenueByTypeReportModel
    {
        [Display(Name = "Event Tickets")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal BidderTickets { get; set; }

        [Display(Name = "Raffle Tickets Purchased in Registration")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal RaffleTicketsViaRegistration { get; set; }

        [Display(Name = "Raffle Tickets Purchased in Store")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal RaffleTicketsViaStore { get; set; }

        [Display(Name = "Store Sales Purchased in Registration")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal StoreSalesViaRegistration { get; set; }

        [Display(Name = "Store Sales Purchased in Store")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal StoreSalesViaStore { get; set; }

        [Display(Name = "Auction Items")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AuctionItems { get; set; }

        [Display(Name = "Total Raised")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total
        {
            get { return BidderTickets + RaffleTicketsViaRegistration + RaffleTicketsViaStore + StoreSalesViaRegistration + StoreSalesViaStore + AuctionItems; }
        }
    }
}