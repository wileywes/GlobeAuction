using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsMasterItemForMultipleWinners { get; set; }

        public virtual Invoice Invoice { get; set; }
    }

    public class DonationItem
    {
        [Key]
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
        public bool HasWinnerBeenEmailed { get; set; }
        public bool UseDigitalCertificateForWinner { get; set; }

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
}