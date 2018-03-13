using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace GlobeAuction.Models
{
    public class Invoice
    {
        [Required]
        [Display(Name = "Invoice #")]
        public int InvoiceId { get; set; }
        [Required]
        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; }
        [Required]
        [Display(Name = "Marked Paid Manually")]
        public bool WasMarkedPaidManually { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "Create Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Update Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime UpdateDate { get; set; }
        [Display(Name = "Updated By")]
        public string UpdateBy { get; set; }

        public virtual Bidder Bidder { get; set; }
        public virtual List<AuctionItem> AuctionItems { get; set; }
        public virtual List<StoreItemPurchase> StoreItemPurchases { get; set; }
        public virtual PayPalTransaction PaymentTransaction { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Zip")]
        public string ZipCode { get; set; }

        public decimal Total
        {
            get
            {
                var total = 0m;
                if (AuctionItems != null && AuctionItems.Any()) total = AuctionItems.Sum(a => a.WinningBid.GetValueOrDefault(0));
                if (StoreItemPurchases != null && StoreItemPurchases.Any()) total += StoreItemPurchases.Sum(sip => sip.PricePaid.GetValueOrDefault(sip.Price * sip.Quantity));
                return total;
            }
        }

        [Display(Name = "Total Paid")]
        public decimal TotalPaid
        {
            get
            {
                if (PaymentTransaction != null)
                {
                    return PaymentTransaction.PaymentGross;
                }
                if (WasMarkedPaidManually)
                {
                    return Total;
                }
                return 0;
            }
        }
    }


    public class InvoiceForPayPal
    {
        public int InvoiceId { get; set; }
        public string PayPalBusiness { get; set; }
        public List<PayPalPaymentLine> LineItems { get; set; }

        public InvoiceForPayPal(Invoice invoice)
        {
            InvoiceId = invoice.InvoiceId;
            PayPalBusiness = ConfigurationManager.AppSettings["PayPalBusiness"];

            LineItems = invoice.AuctionItems.Select(g => new PayPalPaymentLine(g.Title, g.WinningBid.Value, 1)).ToList();
            LineItems.AddRange(invoice.StoreItemPurchases.Select(s => new PayPalPaymentLine(s.StoreItem.Title, s.Price, s.Quantity)));
        }
    }

    public enum PaymentMethod
    {
        PayPal = 0,
        Cash = 1,
        Check = 2,
        PayPalHere = 3
    }

    public enum PayPalTransactionType
    {
        BidderCart,
        InvoiceCart,
        InvoicePayPalHere,
        Unknown
    }

    public enum PayPalNotificationType
    {
        Ipn,
        CartRedirect
    }

    public class PayPalTransaction
    {
        [Required]
        public int PayPalTransactionId { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }
        [Required]
        public string PayerId { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PaymentGross { get; set; }
        [Required]
        public string PaymentType { get; set; }

        public string PayerEmail { get; set; }
        public string PayerStatus { get; set; }

        [Required]
        public string TxnId { get; set; }
        [Required]
        public string PaymentStatus { get; set; }
        [Required]
        public PayPalTransactionType TransactionType { get; set; }
        [Required]
        public PayPalNotificationType NotificationType { get; set; }
        public string IpnTrackId { get; set; }
        public string Custom { get; set; }

        public bool WasPaymentSuccessful { get { return PaymentStatus == "Completed"; } }
        
        public PayPalTransaction()
        {
            //empty constructor for AutoMapper
        }

        public PayPalTransaction(FormCollection form)
        {
            //https://developer.paypal.com/docs/classic/paypal-payments-standard/integration-guide/Appx_websitestandard_htmlvariables/#individual-items-variables
            /*
"payment_date"
"payer_id" - "F22UAR3MXJBFU"
"payment_gross"
"payment_type"
"payer_email"
"payer_status" = "VERIFIED"
"txn_id"
"item_name"
"payment_status" = "Completed"
txn_type=cart
custom=

             */
            PaymentDate = DateTime.Now;
            PayerId = form["payer_id"];
            PaymentGross = decimal.Parse(form["payment_gross"]);
            PaymentType = form["payment_type"];
            PayerEmail = form["payer_email"] ?? form["first_name"] + " " + form["last_name"];
            PayerStatus = form["payer_status"];
            TxnId = form["txn_id"];
            PaymentStatus = form["payment_status"];
            Custom = form["custom"];
            IpnTrackId = form["ipn_track_id"];

            TransactionType = PayPalTransactionType.Unknown;
            if (form.AllKeys.Contains("txn_type"))
            {
                var txn_type = form["txn_type"] ?? string.Empty;
                if (txn_type.Equals("paypal_here"))
                {
                    TransactionType = PayPalTransactionType.InvoicePayPalHere;
                }
                else if (!string.IsNullOrEmpty(Custom) && Custom.StartsWith("Bidder"))
                {
                    TransactionType = PayPalTransactionType.BidderCart;
                }
                else if (!string.IsNullOrEmpty(Custom) && Custom.StartsWith("Invoice"))
                {
                    TransactionType = PayPalTransactionType.InvoiceCart;
                }
            }

            NotificationType = form.AllKeys.Contains("ipn_track_id") ? PayPalNotificationType.Ipn : PayPalNotificationType.CartRedirect;
        }
    }
}