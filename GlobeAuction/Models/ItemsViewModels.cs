using GlobeAuction.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int BidderId { get; set; }

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
        [Required]
        [Display(Name = "Checkout Text Sent")]
        public bool IsCheckoutNudgeTextSent { get; set; }

        public List<AuctionItemViewModel> ItemsWon { get; set; }

        public WinnerViewModel(Bidder bidder, List<AuctionItem> itemsWon)
        {
            this.BidderId = bidder.BidderId;
            this.FirstName = bidder.FirstName;
            this.LastName = bidder.LastName;
            this.Email = bidder.Email;
            this.Phone = bidder.Phone;
            this.IsCheckoutNudgeEmailSent = bidder.IsCheckoutNudgeEmailSent;
            this.IsCheckoutNudgeTextSent = bidder.IsCheckoutNudgeTextSent;
            this.ItemsWon = itemsWon.Select(i => new AuctionItemViewModel(i)).ToList();
            AreWinningsAllPaidFor = itemsWon.All(w => w.Invoice != null && w.Invoice.IsPaid);
        }
    }

    public class EnterWinnersViewModel
    {
        public string SelectedCategory { get; set; }
        public AuctionItemViewModel NextAuctionItemWithNoWinner { get; set; }
    }

    public class NotifyAllWinnersViewModel
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
        public bool IsGiftCard { get; set; }

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
            this.IsGiftCard = item.IsGiftCard;
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
        
        public int DonationItemsCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public int DonationItemsTotalValue { get; set; }

        public int? WinningBidderId { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? WinningBid { get; set; }

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
            this.DonationItemsCount = i.DonationItems.Count;
            this.DonationItemsTotalValue = i.DonationItems.Sum(d => d.DollarValue.GetValueOrDefault(0));
            this.WinningBid = i.WinningBid;
            this.WinningBidderId = i.WinningBidderId;
        }
    }

    public class AuctionItem
    {
        public int AuctionItemId { get; set; }

        /// <summary>
        /// Entered by admin, uniqueness enforced by DB
        /// </summary>
        [Required]
        [Index("IX_AuctionItem_UniqueItemNumber", 1, IsUnique = true)]
        public int UniqueItemNumber { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int StartingBid { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int BidIncrement { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        /// <summary>
        /// List of items in this buyable group - could be 1 or more donation items
        /// </summary>
        public virtual List<DonationItem> DonationItems { get; set; }
        
        public int? WinningBidderId { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? WinningBid { get; set; }

        public virtual Invoice Invoice { get; set; }
    }

    public class DonationItem
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
        public bool HasDisplay { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool IsDeleted { get; set; }

        [Required]
        public bool IsGiftCard { get; set; }

        public virtual Solicitor Solicitor { get; set; }
        public virtual Donor Donor { get; set; }

        public DonationItem Clone()
        {
            var newItem = (DonationItem)MemberwiseClone();
            newItem.DonationItemId = 0;
            newItem.Donor = this.Donor;
            newItem.Solicitor = this.Solicitor;
            return newItem;
        }
    }

    public class Donor
    {
        public int DonorId { get; set; }

        [Required]
        public string BusinessName { get; set; }

        [Required]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Zip { get; set; }

        [Required]
        public string ContactName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public virtual List<DonationItem> DonationItems { get; set; }
    }

    public class Solicitor
    {
        public int SolicitorId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public virtual List<DonationItem> DonationItems { get; set; }
    }
}