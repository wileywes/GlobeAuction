using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GlobeAuction.Models
{
    public class ItemsViewModel
    {
        public List<AuctionItemViewModel> AuctionItems { get; set; }
        public List<DonationItemViewModel> DonationsNotInAuctionItem { get; set; }
    }
    
    public class WinnerViewModel
    {
        [Display(Name = "ID")]
        public int BidderId { get; set; }

        [Required]
        [Display(Name = "Bidder #")]
        public int BidderNumber { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        public bool AreWinningsAllPaidFor { get; set; }
        
        [Required]
        [Display(Name = "Checkout Email Sent")]
        public bool IsCheckoutNudgeEmailSent { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Required]
        public decimal? TotalUnpaid { get; set; }

        [Display(Name = "Checkout Text Sent")]
        public bool IsCheckoutNudgeTextSent { get; set; }

        public List<BidViewModel> ItemsWon { get; set; }

        public WinnerViewModel(Bidder bidder, List<Bid> itemsWon)
        {
            this.BidderId = bidder.BidderId;
            this.BidderNumber = bidder.BidderNumber;
            this.FirstName = bidder.FirstName;
            this.LastName = bidder.LastName;
            this.Email = bidder.Email;
            this.Phone = bidder.Phone;
            this.IsCheckoutNudgeEmailSent = bidder.IsCheckoutNudgeEmailSent;
            this.IsCheckoutNudgeTextSent = bidder.IsCheckoutNudgeTextSent;
            this.ItemsWon = itemsWon.Select(i => new BidViewModel(i)).ToList();
            AreWinningsAllPaidFor = itemsWon.All(w => w.Invoice != null && w.Invoice.IsPaid);

            if (!AreWinningsAllPaidFor)
            {
                TotalUnpaid = itemsWon.Where(w => w.Invoice == null || !w.Invoice.IsPaid).Sum(i => i.BidAmount);
            }
        }
    }

    public class EnterWinnersInBulkViewModel
    {
        [Display(Name = "Auction Item")]
        public int SelectedAuctionItemId { get; set; }
        public decimal BidPrice { get; set; }
        public string ListOfBidderNumbers { get; set; }

        //outputs
        public int? ItemsCreated { get; set; }
        public List<string> ErrorMessages { get; set; }
    }

    public class NotifyResultViewModel
    {
        public bool WasSuccessful { get { return MessagesFailed == 0; } }
        public string ErrorMessage { get; set; }
        public int MessagesSent { get; set; }
        public int MessagesFailed { get; set; }
    }

    public class DonationItemViewModel
    {
        public int DonationItemId { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public string Restrictions { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExpirationDate { get; set; }
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int? DollarValue { get; set; }
        public string HasDisplay { get; set; }
        [Required]
        public int Quantity { get; set; }
        public bool HasWinnerBeenEmailed { get; set; }
        public string UseDigitalCertificateForWinner { get; set; }

        public DonationItemViewModel()
        {
        }

        public DonationItemViewModel(DonationItem item)
        {
            this.Category = item.Category;
            this.Title = item.Title;
            this.Description = item.Description; //.TruncateTo(50);
            this.DollarValue = item.DollarValue;
            this.DonationItemId = item.DonationItemId;
            this.ExpirationDate = item.ExpirationDate;
            this.Restrictions = item.Restrictions; //.TruncateTo(50);
            this.HasDisplay = item.HasDisplay ? "Yes" : "No";
            this.UseDigitalCertificateForWinner = item.UseDigitalCertificateForWinner ? "Yes" : "No";
            this.Quantity = item.Quantity;
        }
    }

    public class AuctionItemViewModel
    {
        public int AuctionItemId { get; set; }
        public int UniqueItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int StartingBid { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int BidIncrement { get; set; }
        [Required]
        public int Quantity { get; set; }

        public List<DonationItem> DonationItems { get; set; }
        public int DonationItemsCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public int DonationItemsTotalValue { get; set; }
        
        public List<BidViewModel> AllBids { get; set; }

        public AuctionItemViewModel()
        {
            //empty for model binding
        }

        public AuctionItemViewModel(AuctionItem i)
        {
            this.AuctionItemId = i.AuctionItemId;
            this.UniqueItemNumber = i.UniqueItemNumber;
            this.Title = i.Title;
            this.Description = i.Description; //.TruncateTo(50);
            this.Category = i.Category;
            this.StartingBid = i.StartingBid;
            this.BidIncrement = i.BidIncrement;
            this.Quantity = i.Quantity;
            this.DonationItems = i.DonationItems;
            this.DonationItemsCount = i.DonationItems.Count;
            this.DonationItemsTotalValue = i.DonationItems.Sum(d => d.DollarValue.GetValueOrDefault(0));
            this.AllBids = i.AllBids.Select(b => new BidViewModel(b)).ToList();
        }
    }


    public class BidViewModel
    {
        public int BidId { get; set; }

        public virtual AuctionItemViewModel AuctionItem { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal BidAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? AmountPaid { get; set; }

        public bool IsPaid { get { return AmountPaid.HasValue; } }

        public bool IsWinning { get; set; }

        public BidViewModel(Bid bid)
        {
            BidId = bid.BidId;
            AuctionItem = new AuctionItemViewModel(bid.AuctionItem);
            BidAmount = bid.BidAmount;
            AmountPaid = bid.AmountPaid;
            IsWinning = bid.IsWinning;
        }
    }
}