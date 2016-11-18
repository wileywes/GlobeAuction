using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace GlobeAuction.Models
{
    public class BidderForList
    {
        public int BidderId { get; set; }

        [Required]
        [Display(Name ="First Name")]
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

        [Display(Name = "Zip")]
        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; }

        [Display(Name = "Guests")]
        public int GuestCount { get; set; }

        [Display(Name = "Tickets Purchased")]
        public int TicketsPaid { get; set; }

        [Display(Name = "Tickets Cost Paid")]
        public decimal TotalTicketCostPaid { get; set; }

        public BidderForList(Bidder b)
        {
            BidderId = b.BidderId;
            FirstName = b.FirstName;
            LastName = b.LastName;
            Phone = b.Phone;
            Email = b.Email;
            ZipCode = b.ZipCode;

            if (b.AuctionGuests.Any())
            {
                GuestCount = b.AuctionGuests.Count;
                TicketsPaid = b.AuctionGuests.Count(g => g.IsTicketPaid);
                TotalTicketCostPaid = b.AuctionGuests.Sum(g => g.TicketPricePaid.GetValueOrDefault(0));
            }
        }
    }

    public class BidderForPayPal
    {
        public int BidderId { get; set; }
        public List<BidderTicket> Tickets { get; set; }

        public BidderForPayPal(Bidder bidder)
        {
            BidderId = bidder.BidderId;
            
            if (bidder.AuctionGuests == null || !bidder.AuctionGuests.Any()) throw new ApplicationException("No auction guests found");

            Tickets = bidder.AuctionGuests.Select(g => new BidderTicket(g)).ToList();
        }
    }

    public class BidderTicket
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public BidderTicket(AuctionGuest guest)
        {
            Name = guest.TicketType;
            Price = guest.TicketPrice;
        }
    }

    public class Bidder
    {
        public int BidderId { get; set; }

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

        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; }

        public List<AuctionGuest> AuctionGuests { get; set; }
        public List<Student> Students { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        [Required]
        public bool IsPaymentReminderSent { get; set; }
    }

    public class AuctionGuest
    {
        public int AuctionGuestId { get; set; }        
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [RequiredIfHasValue("FirstName", ErrorMessage = "You must select a Ticket Type to purchase")]
        public string TicketType { get; set; }
        public decimal TicketPrice { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? TicketPricePaid { get; set; }

        public bool IsTicketPaid {  get { return TicketPricePaid.HasValue; } }

        public Bidder Bidder { get; set; }
        public PayPalTransaction TicketTransaction { get; set; }
    }

    public class Student
    {
        public int StudentId { get; set; }
        public string HomeroomTeacher { get; set; }

        public Bidder Bidder { get; set; }
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

        public bool WasPaymentSuccessful {  get { return PaymentStatus == "Completed"; } }

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
             */
            PaymentDate = DateTime.Now;
            PayerId = form["payer_id"];
            PaymentGross = decimal.Parse(form["payment_gross"]);
            PaymentType = form["payment_type"];
            PayerEmail = form["payer_email"];
            PayerStatus = form["payer_status"];
            TxnId = form["txn_id"];
            PaymentStatus = form["payment_status"];
        }
    }
}