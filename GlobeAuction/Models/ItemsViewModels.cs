using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GlobeAuction.Models
{
    public class ItemsViewModel
    {
        public AuctionItem EmptyAuctionItem = new AuctionItem();
        public DonationItemViewModel EmptyDonationItem = new DonationItemViewModel();

        public List<AuctionItem> AuctionItems { get; set; }
        public List<DonationItemViewModel> DonationsNotInAuctionItem { get; set; }
    }

    public class DonationItemViewModel
    {
        public bool IsSelected { get; set; }
        public int DonationItemId { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public string Restrictions { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }
        [DataType(DataType.Currency)]
        public int? DollarValue { get; set; }

        public DonationItemViewModel()
        {
        }

        public DonationItemViewModel(DonationItem item)
        {
            this.Category = item.Category;
            this.Description = item.Description;
            this.DollarValue = item.DollarValue;
            this.DonationItemId = item.DonationItemId;
            this.ExpirationDate = item.ExpirationDate;
            this.Restrictions = item.Restrictions;
        }
    }

    public class AuctionItem
    {
        public int AuctionItemId { get; set; }

        /// <summary>
        /// Entered by admin, uniqueness enforced by DB
        /// </summary>
        [Required]
        public int UniqueItemNumber { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public int StartingBid { get; set; }
        [Required]
        public int BidIncrement { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        /// <summary>
        /// List of items in this buyable group - could be 1 or more donation items
        /// </summary>
        public List<DonationItem> DonationItems { get; set; }
        
        public int WinningBidderId { get; set; }
        public int WinningBid { get; set; }
    }

    public class DonationItem
    {
        public int DonationItemId { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string Restrictions { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [DataType(DataType.Currency)]
        public int? DollarValue { get; set; }

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