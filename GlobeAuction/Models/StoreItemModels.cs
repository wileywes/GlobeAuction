using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobeAuction.Models
{
    public class StoreItemViewModel
    {
        public int StoreItemId { get; set; }

        [Required]
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
        [Display(Name = "Available Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Is Raffle Ticket")]
        public bool IsRaffleTicket { get; set; }

        [Display(Name = "Is Bundle of Raffle Tickets")]
        public bool IsBundleParent { get; set; }
        public virtual List<BundleComponent> BundleComponents { get; set; }

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

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreateDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        //reference to donation item that the StoreItem was copied from.  Null for items not converted from donation items
        public virtual DonationItem DonationItem { get; set; }
    }

    public class StoreItemsListViewModel : StoreItemViewModel
    {
        public int UnpaidPurchaseCount { get; set; }
        public int PaidPurchaseCount { get; set; }
    }

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

    public class StoreItemPurchaseViewModel
    {
        public int StoreItemPurchaseId { get; set; }

        [Required]
        public StoreItemViewModel StoreItem { get; set; }

        public int Quantity { get; set; }
        
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? PricePaid { get; set; }

        public bool IsPaid { get { return PricePaid.HasValue; } }

        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get
            {
                if (Bidder != null) return Bidder.Email;
                if (Invoice != null) return Invoice.Email;
                return string.Empty;
            }
        }

        public string FullName
        {
            get
            {
                if (Bidder != null) return Bidder.FirstName + " " + Bidder.LastName;
                if (Invoice != null) return Invoice.FirstName + " " + Invoice.LastName;
                return string.Empty;
            }
        }
        public DateTime PurchaseDate
        {
            get
            {
                if (Bidder != null) return Bidder.CreateDate;
                if (Invoice != null) return Invoice.CreateDate;
                return DateTime.MinValue;
            }
        }

        public string PurchaseType { get { return Bidder == null ? "Store" : "Bidder"; } }

        public virtual PayPalTransaction PurchaseTransaction { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Bidder Bidder { get; set; }
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

        public virtual PayPalTransaction PurchaseTransaction { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Bidder Bidder { get; set; }
    }

    public class BuyViewModel
    {
        public int? BidderId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PostalCode)]
        [Display(Name = "Zip")]
        public string ZipCode { get; set; }

        public List<StoreItemPurchaseViewModel> StoreItemPurchases { get; set; }

        public bool ShowInvoiceCreatedSuccessMessage { get; set; }
        public int? InvoiceIdCreated { get; set; }
        public string InvoiceFullNameCreated { get; set; }
    }
}