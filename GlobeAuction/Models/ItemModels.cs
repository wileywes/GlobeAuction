using GlobeAuction.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GlobeAuction.Models
{
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

        public virtual AuctionCategory Category { get; set; }
        
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int StartingBid { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int BidIncrement { get; set; }
        [Required]
        public bool IsFixedPrice { get; set; }
        [Required]
        public bool IsInFiresale { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        /// <summary>
        /// List of items in this buyable group - could be 1 or more donation items
        /// </summary>
        public virtual List<DonationItem> DonationItems { get; set; }
        public virtual List<Bid> AllBids { get; set; }

        public IEnumerable<Bid> WinningBids
        {
            get
            {
                return AllBids.Where(b => b.IsWinning);
            }
        }
    }

    public class DonationItem
    {
        [Key]
        public int DonationItemId { get; set; }

        public virtual AuctionCategory Category { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        public string Restrictions { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExpirationDate { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int? DollarValue { get; set; }
        public bool HasDisplay { get; set; }
        public bool IsReceived { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsInStore { get; set; }
        public bool UseDigitalCertificateForWinner { get; set; }
        public string DigitalCertificateUrl { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public virtual Solicitor Solicitor { get; set; }
        public virtual Donor Donor { get; set; }
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

        public bool HasTaxReceiptBeenEmailed { get; set; }

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

    public class Bid
    {
        public int BidId { get; set; }

        [Required]  //<======= Forces Cascade delete
        public virtual AuctionItem AuctionItem { get; set; }

        [Required]  //<======= Forces Cascade delete
        public virtual Bidder Bidder { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal BidAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal? AmountPaid { get; set; }

        public bool IsPaid { get { return AmountPaid.HasValue; } }

        [Required]
        public bool IsWinning { get; set; }
        public bool HasWinnerBeenEmailed { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        public virtual Invoice Invoice { get; set; }
    }

    public class AuctionCategory
    {
        public int AuctionCategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Open Bidding (EST)")]
        public DateTime BidOpenDateLtz { get; set; }

        [Required]
        [Display(Name = "Close Bidding (EST)")]
        public DateTime BidCloseDateLtz { get; set; }

        [Required]
        [Display(Name = "Fund-a-Project")]
        public bool IsFundAProject { get; set; }

        [Display(Name = "Allows Mobile Bidding")]
        public bool IsAvailableForMobileBidding { get; set; }

        /// <summary>
        /// Should be true for "Live" category
        /// </summary>
        [Required]
        [Display(Name = "Hide from Donate Form")]
        public bool IsOnlyAvailableToAuctionItems { get; set; }

        [Display(Name = "Starting Item No.")]
        public int? ItemNumberStart { get; set; }

        [Display(Name = "Ending Item No.")]
        public int? ItemNumberEnd { get; set; }

        public bool IsBiddingOpen
        {
            get
            {
                var localTime = Utilities.GetEasternTimeNow();
                return IsAvailableForMobileBidding && localTime > BidOpenDateLtz && localTime < BidCloseDateLtz;
            }
        }
    }
}