using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public bool HasUnlimitedQuantity { get; set; }

        [Required]
        [Display(Name = "Is Raffle Ticket")]
        public bool IsRaffleTicket { get; set; }

        [Display(Name = "Is Bundle of Raffle Tickets")]
        public bool IsBundleParent { get; set; }
        public virtual List<BundleComponent> BundleComponents { get; set; }

        [Required]
        [Display(Name = "Show in Bidder Registration")]
        public bool CanPurchaseInBidderRegistration { get; set; }

        [Required]
        [Display(Name = "Show in Auction Checkout")]
        public bool CanPurchaseInAuctionCheckout { get; set; }

        [Required]
        [Display(Name = "Show in Store")]
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
        [Display(Name = "Unpaid Count")]
        public int UnpaidPurchaseCount { get; set; }
        [Display(Name = "Paid Count")]
        public int PaidPurchaseCount { get; set; }
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

        public bool IsRafflePrinted { get; set; }

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

        public List<BuyItemViewModel> RaffleItems { get; set; }
        public List<BuyItemViewModel> StoreItems { get; set; }

        public bool ShowInvoiceCreatedSuccessMessage { get; set; }
        public int? InvoiceIdCreated { get; set; }
        public string InvoiceFullNameCreated { get; set; }
        public List<string> RaffleTicketNumbersCreated { get; set; }
    }

    public class BuyItemViewModel
    {
        //store item info
        public int StoreItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public bool HasUnlimitedQuantity { get; set; }
        public decimal? DonationItemValue { get; set; }

        //input from user
        public int? QuantityPurchased { get; set; }

        /// <summary>
        /// For model binding only!!
        /// </summary>
        public BuyItemViewModel()
        {
        }

        public BuyItemViewModel(StoreItem si)
        {
            StoreItemId = si.StoreItemId;
            Title = si.Title;
            Description = si.Description;
            ImageUrl = si.ImageUrl;
            Price = si.Price;
            QuantityAvailable = si.Quantity;
            HasUnlimitedQuantity = si.HasUnlimitedQuantity;
            DonationItemValue = si?.DonationItem?.DollarValue;
        }
    }
}