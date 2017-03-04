using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobeAuction.Models
{
    public class StoreItemViewModel
    {
        public int StoreItemId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Can Purchase in Bidder Registration")]
        public bool CanPurchaseInBidderRegistration { get; set; }

        [Required]
        [Display(Name = "Can Purchase in Auction Checkout")]
        public bool CanPurchaseInAuctionCheckout { get; set; }

        [Required]
        [Display(Name = "Can Purchase in Store")]
        public bool CanPurchaseInStore { get; set; }

        [Required]
        [Display(Name = "Only for Admins")]
        public bool OnlyVisibleToAdmins { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }

    public class StoreItem
    {
        public int StoreItemId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(500)]
        [Index("IX_StoreItem_Title", IsUnique = true)]
        public string Title { get; set; }
        
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [Required]
        public bool CanPurchaseInBidderRegistration { get; set; }

        [Required]
        public bool CanPurchaseInAuctionCheckout { get; set; }

        [Required]
        public bool CanPurchaseInStore { get; set; }

        [Required]
        public bool OnlyVisibleToAdmins { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }        
    }

    public class StoreItemPurchaseViewModel
    {
        public int StoreItemPurchaseId { get; set; }

        public StoreItemViewModel StoreItem { get; set; }

        [RequiredIfHasValue("Quantity", ErrorMessage = "You must select an item if entering a quantity to purchase.")]
        public int? StoreItemId { get; set;  }

        [RequiredIfHasValue("StoreItemId", ErrorMessage = "Quantity is required if you select an item to purchase.")]
        public int? Quantity { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? PricePaid { get; set; }

        public bool IsPaid { get { return PricePaid.HasValue; } }

        public virtual PayPalTransaction PurchaseTransaction { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Bidder Bidder { get; set; }
    }

    public class StoreItemPurchase
    {
        [Required]
        public int StoreItemPurchaseId { get; set; }

        [Required]
        public virtual StoreItem StoreItem { get; set; }

        [Required]
        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? PricePaid { get; set; }

        public bool IsPaid { get { return PricePaid.HasValue; } }

        public virtual PayPalTransaction PurchaseTransaction { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Bidder Bidder { get; set; }
    }
}