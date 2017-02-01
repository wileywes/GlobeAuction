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
        public string PostActionName { get; set; }
        public string SelectedDonationItemIds { get; set; }
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

        public DonationItemViewModel()
        {
        }

        public DonationItemViewModel(DonationItem item)
        {
            this.Category = item.Category;
            this.Title = item.Title;
            this.Description = item.Description.TruncateTo(50);
            this.DollarValue = item.DollarValue;
            this.DonationItemId = item.DonationItemId;
            this.ExpirationDate = item.ExpirationDate;
            this.Restrictions = item.Restrictions.TruncateTo(50);
        }
    }

    public class AuctionItemViewModel
    {
        public int AuctionItemId { get; set; }
        public int UniqueItemNumber { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int StartingBid { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int BidIncrement { get; set; }
        public List<DonationItemViewModel> DonationItems { get; set; }
        public int? WinningBidderId { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public int? WinningBid { get; set; }

        public AuctionItemViewModel(AuctionItem i)
        {
            this.AuctionItemId = i.AuctionItemId;
            this.UniqueItemNumber = i.UniqueItemNumber;
            this.Description = i.Description.TruncateTo(50);
            this.Category = i.Category;
            this.StartingBid = i.StartingBid;
            this.BidIncrement = i.BidIncrement;
            this.DonationItems = i.DonationItems.Select(d => new DonationItemViewModel(d)).ToList();
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
        public List<DonationItem> DonationItems { get; set; }
        
        public int? WinningBidderId { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public int? WinningBid { get; set; }
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

        public Solicitor Solicitor { get; set; }
        public Donor Donor { get; set; }

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

        public List<DonationItem> DonationItems { get; set; }
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

        public List<DonationItem> DonationItems { get; set; }
    }
}