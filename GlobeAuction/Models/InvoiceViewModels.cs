﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GlobeAuction.Models
{
    public class InvoiceLookupModel
    {
        [Required]
        [Display(Name = "Bidder #")]
        public int? BidderNumber { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }

    public class ReviewBidderWinningsViewModel
    {
        public int BidderId { get; set; }
        [Display(Name = "Bidder #")]
        public int BidderNumber { get; set; }
        public string BidderName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string BidderEmail { get; set; }

        public List<Invoice> Invoices { get; set; }
        public List<AuctionItemViewModel> AuctionItemsNotInInvoice { get; set; }
        public List<BuyItemViewModel> ItemPurchases { get; set; }

        public bool ShowManuallyPaidSuccess { get; set; }
        public bool ShowSelfPaySuccess { get; set; }

        public ReviewBidderWinningsViewModel()
        {
            //empty constr for view binding
        }

        public ReviewBidderWinningsViewModel(Bidder bidder, List<Invoice> invoices, List<AuctionItem> auctionWinningsForBidderNotInInvoice, List<BuyItemViewModel> storeItems)
        {
            BidderId = bidder.BidderId;
            BidderNumber = bidder.BidderNumber;
            BidderName = bidder.FirstName + "  " + bidder.LastName;
            BidderEmail = bidder.Email;

            Invoices = (invoices ?? new List<Invoice>()).ToList();
            AuctionItemsNotInInvoice = auctionWinningsForBidderNotInInvoice.Select(a => new AuctionItemViewModel(a, bidder.BidderNumber)).ToList();
            ItemPurchases = storeItems.ToList();
        }
    }

    public class InvoiceListViewModel
    {
        [Display(Name = "Invoice #")]
        public int InvoiceId { get; set; }

        [Display(Name = "Paid")]
        public bool IsPaid { get; set; }

        [Display(Name = "Paid Marked Manually")]
        public bool WasMarkedPaidManually { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        public int? BidderId { get; set; }

        [Display(Name = "Bidder #")]
        public int? BidderNumber { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }

        [Display(Name = "# Items")]
        public int CountOfItems { get; set; }

        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal InvoiceTotal { get; set; }

        [Display(Name = "Total Paid")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal InvoiceTotalPaid { get; set; }

        [Display(Name = "Create Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreateDate { get; set; }


        public InvoiceListViewModel(Invoice invoice)
        {
            if (invoice.Bidder != null)
            {
                BidderId = invoice.Bidder.BidderId;
                BidderNumber = invoice.Bidder.BidderNumber;
            }
            
            Name = invoice.FirstName + "  " + invoice.LastName;
            Email = invoice.Email;
            InvoiceId = invoice.InvoiceId;
            IsPaid = invoice.IsPaid;
            PaymentMethod = invoice.PaymentMethod;
            WasMarkedPaidManually = invoice.WasMarkedPaidManually;
            CreateDate = invoice.CreateDate;

            invoice.AuctionItems = invoice.AuctionItems ?? new List<AuctionItem>();
            invoice.StoreItemPurchases = invoice.StoreItemPurchases ?? new List<StoreItemPurchase>();

            CountOfItems = invoice.AuctionItems.Count + invoice.StoreItemPurchases.Count;
            InvoiceTotal = invoice.Total;

            InvoiceTotalPaid = invoice.TotalPaid;
        }
    }
}