using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;

namespace GlobeAuction.Models
{
    public class BidderForList
    {
        [Display(Name = "ID")]
        public int BidderId { get; set; }

        [Required]
        [Display(Name = "Bidder #")]
        public int BidderNumber { get; set; }

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

        [Display(Name = "Zip")]
        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; }

        [Display(Name = "Guests")]
        public int GuestCount { get; set; }

        [Display(Name = "Tickets Paid For")]
        public int TicketsPaid { get; set; }


        [Display(Name = "Items")]
        public int ItemsCount { get; set; }

        [Display(Name = "Items Paid For")]
        public int ItemsPaid { get; set; }


        [Display(Name = "Total Paid")]
        public decimal TotalPaid { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "Attended")]
        public bool AttendedEvent { get; set; }

        [Display(Name = "Pay Reminder Sent")]
        public bool IsPaymentReminderSent { get; set; }

        [Display(Name = "Registration Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreateDate { get; set; }

        public BidderForList(Bidder b, Invoice i)
        {
            BidderId = b.BidderId;
            BidderNumber = b.BidderNumber;
            FirstName = b.FirstName;
            LastName = b.LastName;
            Phone = b.Phone;
            Email = b.Email;
            ZipCode = b.ZipCode;
            IsPaymentReminderSent = b.IsPaymentReminderSent;
            CreateDate = b.CreateDate;

            AttendedEvent = b.AttendedEvent;

            if (i != null)
            {
                TotalPaid = i.TotalPaid;
                PaymentMethod = i.PaymentMethod;

                if (i.TicketPurchases.Any())
                {
                    GuestCount = i.TicketPurchases.Count;
                    TicketsPaid = i.TicketPurchases.Count(g => g.IsTicketPaid);
                }

                if (i.StoreItemPurchases.Any())
                {
                    ItemsCount = i.StoreItemPurchases.Count;
                    ItemsPaid = i.StoreItemPurchases.Count(g => g.IsPaid);
                }
            }
        }
    }

    public class BidderForPayPal
    {
        public int BidderId { get; set; }
        public int InvoiceId { get; set; }
        public string PayPalBusiness { get; set; }
        public List<PayPalPaymentLine> LineItems { get; set; }

        public BidderForPayPal(Bidder bidder, Invoice invoice)
        {
            BidderId = bidder.BidderId;
            InvoiceId = invoice.InvoiceId;
            PayPalBusiness = ConfigurationManager.AppSettings["PayPalBusiness"];

            if (bidder.AuctionGuests == null || !bidder.AuctionGuests.Any()) throw new ApplicationException("No auction guests found");

            LineItems = invoice.TicketPurchases.Select(g => new PayPalPaymentLine(g.TicketType, g.TicketPrice, 1)).ToList();
            LineItems.AddRange(invoice.StoreItemPurchases.Select(s => new PayPalPaymentLine(s.StoreItem.Title, s.Price, s.Quantity)));
        }
    }

    public class PayPalPaymentLine
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public PayPalPaymentLine(string name, decimal price, int quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }
    }

    public class BidderViewModel
    {
        [Display(Name = "ID")]
        public int BidderId { get; set; }

        [Required]
        [Display(Name = "Bidder #")]
        public int BidderNumber { get; set; }

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

        public List<AuctionGuestViewModel> AuctionGuests { get; set; }
        public List<StudentViewModel> Students { get; set; }
        public Invoice RegistrationInvoice { get; set; }

        [Display(Name = "Registration Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreateDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

        [Required]
        [Display(Name = "Pay Reminder Sent")]
        public bool IsPaymentReminderSent { get; set; }

        [Required]
        [Display(Name = "Checkout Email Sent")]
        public bool IsCheckoutNudgeEmailSent { get; set; }
        [Required]
        [Display(Name = "Checkout Text Sent")]
        public bool IsCheckoutNudgeTextSent { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "Attended")]
        public bool AttendedEvent { get; set; }
    }

    public class BidderRegistrationViewModel
    {
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

        public List<AuctionGuestViewModel> AuctionGuests { get; set; }
        public List<StudentViewModel> Students { get; set; }
        public List<BuyItemViewModel> ItemPurchases { get; set; }

        //shown if registered and marked manually
        public bool ShowRegistrationSuccessMessage { get; set; }
        public string FullNameJustRegistered { get; set; }
        public int? BidderNumberJustRegistered { get; set; }
        public List<string> RaffleTicketNumbersCreated { get; set; }
    }

    public class AuctionGuestViewModel
    {
        public int AuctionGuestId { get; set; }

        [RequiredIfHasValue("TicketType", ErrorMessage = "First Name is required for each ticket selected")]
        public string FirstName { get; set; }

        [RequiredIfHasValue("TicketType", ErrorMessage = "Last Name is required for each ticket selected")]
        public string LastName { get; set; }

        [RequiredIfHasValue("FirstName", ErrorMessage = "You must select a Ticket Type for each guest entered")]
        public string TicketType { get; set; }
        public decimal TicketPrice { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? TicketPricePaid { get; set; }

        public bool IsTicketPaid { get { return TicketPricePaid.HasValue; } }

        public AuctionGuestViewModel()
        { }
    }

    public class StudentViewModel
    {
        public int StudentId { get; set; }
        public string HomeroomTeacher { get; set; }

        public StudentViewModel()
        { }

        public StudentViewModel(Student s)
        {
            this.StudentId = s.StudentId;
            this.HomeroomTeacher = s.HomeroomTeacher;
        }
    }

    public class BidderCookieInfo
    {
        public int BidderId { get; set; }
        public int BidderNumber { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}