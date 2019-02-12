using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobeAuction.Models
{
    public class StoreItem
    {
        public int StoreItemId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(500)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public bool HasUnlimitedQuantity { get; set; }

        [Required]
        public bool IsRaffleTicket { get; set; }

        [Required]
        public bool IsBundleParent { get; set; }
        public virtual List<BundleComponent> BundleComponents { get; set; }

        [Required]
        public bool CanPurchaseInBidderRegistration { get; set; }

        [Required]
        public bool CanPurchaseInAuctionCheckout { get; set; }

        [Required]
        public bool CanPurchaseInStore { get; set; }

        [Required]
        public bool OnlyVisibleToAdmins { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        public virtual List<StoreItemPurchase> StoreItemPurchases { get; set; }

        //reference to donation item that the StoreItem was copied from.  Null for items not converted from donation items
        public virtual DonationItem DonationItem { get; set; }
    }

    public class BundleComponent
    {
        public int BundleComponentId { get; set; }

        [Required]
        public int StoreItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal ComponentUnitPrice { get; set; }
        
        //should always exist but not marked required to keep UI view simple
        public StoreItem BundleParent { get; set; }
    }

    public class StoreItemPurchase
    {
        [Required]
        public int StoreItemPurchaseId { get; set; }

        public virtual StoreItem StoreItem { get; set; }

        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? PricePaid { get; set; }

        public bool IsPaid { get { return PricePaid.HasValue; } }

        public bool IsRafflePrinted { get; set; }

        public virtual PayPalTransaction PurchaseTransaction { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Bidder Bidder { get; set; }
    }
}