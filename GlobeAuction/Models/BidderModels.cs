using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public bool IsPaymentReminderSent{ get; set; }

        [Display(Name = "Registration Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreateDate { get; set; }

        public BidderForList(Bidder b)
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

            TotalPaid = b.TotalPaid;
            PaymentMethod = b.PaymentMethod;
            AttendedEvent = b.AttendedEvent;

            if (b.AuctionGuests.Any())
            {
                GuestCount = b.AuctionGuests.Count;
                TicketsPaid = b.AuctionGuests.Count(g => g.IsTicketPaid);
            }

            if (b.StoreItemPurchases.Any())
            {
                ItemsCount = b.StoreItemPurchases.Count;
                ItemsPaid = b.StoreItemPurchases.Count(g => g.IsPaid);
            }
        }
    }

    public class BidderForPayPal
    {
        public int BidderId { get; set; }
        public string PayPalBusiness { get; set; }
        public List<PayPalPaymentLine> LineItems { get; set; }

        public BidderForPayPal(Bidder bidder)
        {
            BidderId = bidder.BidderId;
            PayPalBusiness = ConfigurationManager.AppSettings["PayPalBusiness"];

            if (bidder.AuctionGuests == null || !bidder.AuctionGuests.Any()) throw new ApplicationException("No auction guests found");

            LineItems = bidder.AuctionGuests.Select(g => new PayPalPaymentLine(g.TicketType, g.TicketPrice, 1)).ToList();
            LineItems.AddRange(bidder.StoreItemPurchases.Select(s => new PayPalPaymentLine(s.StoreItem.Title, s.Price, s.Quantity)));
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

    public class Bidder
    {
        public int BidderId { get; set; }
        
        [Index("IX_Bidder_BidderNumber", 1, IsUnique = true)]
        public int BidderNumber { get; set; }

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
                
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool IsDeleted { get; set; }

        [Required]
        public bool IsPaymentReminderSent { get; set; }

        [Required]
        public bool IsCheckoutNudgeEmailSent { get; set; }
        [Required]
        public bool IsCheckoutNudgeTextSent { get; set; }

        [Display(Name = "Marked Paid Manually")]
        public bool WasMarkedPaidManually { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        public bool AttendedEvent { get; set; }

        //children
        public virtual List<AuctionGuest> AuctionGuests { get; set; }
        public virtual List<Student> Students { get; set; }
        public virtual List<StoreItemPurchase> StoreItemPurchases { get; set; }
        

        [Display(Name = "Total Paid")]
        public decimal TotalPaid
        {
            get
            {
                var total = 0m;
                if (AuctionGuests != null && AuctionGuests.Any()) total = AuctionGuests.Sum(g => g.TicketPricePaid.GetValueOrDefault(0));
                if (StoreItemPurchases != null && StoreItemPurchases.Any()) total += StoreItemPurchases.Sum(sip => sip.PricePaid.GetValueOrDefault(sip.Price * sip.Quantity));
                return total;
            }
        }
    }

    public class AuctionGuest
    {
        public int AuctionGuestId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TicketType { get; set; }
        public decimal TicketPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal? TicketPricePaid { get; set; }

        public bool IsTicketPaid {  get { return TicketPricePaid.HasValue; } }

        public virtual Bidder Bidder { get; set; }
        public virtual PayPalTransaction TicketTransaction { get; set; }
    }

    public class Student
    {
        public int StudentId { get; set; }
        public string HomeroomTeacher { get; set; }

        public virtual Bidder Bidder { get; set; }
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
        public List<StoreItemPurchaseViewModel> StoreItemPurchases { get; set; }

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
}