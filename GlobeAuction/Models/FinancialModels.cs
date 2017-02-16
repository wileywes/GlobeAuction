using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace GlobeAuction.Models
{
    public class Invoice
    {
        [Required]
        public int InvoiceId { get; set; }
        [Required]
        public bool IsPaid { get; set; }
        [Required]
        public bool WasMarkedPaidManually { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        [Required]
        public Bidder Bidder { get; set; }

        public List<AuctionItem> AuctionItems { get; set; }
        public PayPalTransaction PaymentTransaction { get; set; }
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
        public decimal PaymentGross { get; set; }
        [Required]
        public string PaymentType { get; set; }
        [Required]
        public string PayerEmail { get; set; }
        [Required]
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
            PayerEmail = form["payer_email"];
            PayerStatus = form["payer_status"];
            TxnId = form["txn_id"];
            PaymentStatus = form["payment_status"];
            Custom = form["custom"];

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