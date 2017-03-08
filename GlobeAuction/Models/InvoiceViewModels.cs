using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GlobeAuction.Models
{
    public class InvoiceLookupModel
    {
        [Required]
        [Display(Name = "Bidder #")]
        public int? BidderId { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }

    public class ReviewBidderWinningsViewModel
    {
        [Display(Name = "Bidder #")]
        public int BidderId { get; set; }
        public string BidderName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string BidderEmail { get; set; }

        public List<Invoice> Invoices { get; set; }
        public List<AuctionItemViewModel> AuctionItemsNotInInvoice { get; set; }

        public ReviewBidderWinningsViewModel(Bidder bidder, IEnumerable<Invoice> invoices, IEnumerable<AuctionItem> auctionWinningsForBidderNotInInvoice)
        {
            BidderId = bidder.BidderId;
            BidderName = bidder.FirstName + "  " + bidder.LastName;
            BidderEmail = bidder.Email;

            Invoices = (invoices ?? new List<Invoice>()).ToList();
            AuctionItemsNotInInvoice = auctionWinningsForBidderNotInInvoice.Select(a => new AuctionItemViewModel(a)).ToList();
        }
    }

    public class InvoiceListViewModel
    {
        public int InvoiceId { get; set; }
        public bool IsPaid { get; set; }
        public bool WasMarkedPaidManually { get; set; }

        public int? BidderId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        [Display(Name = "# Items")]
        public int CountOfAuctionItems { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal InvoiceTotal { get; set; }

        public InvoiceListViewModel(Invoice invoice)
        {
            if (invoice.Bidder != null) BidderId = invoice.Bidder.BidderId;

            Name = invoice.FirstName + "  " + invoice.LastName;
            Email = invoice.Email;

            InvoiceId = invoice.InvoiceId;
            IsPaid = invoice.IsPaid;
            WasMarkedPaidManually = invoice.WasMarkedPaidManually;

            CountOfAuctionItems = invoice.AuctionItems.Count;
            InvoiceTotal = invoice.AuctionItems.Any() ? invoice.AuctionItems.Sum(i => i.WinningBid.Value) : 0;
        }
    }
}