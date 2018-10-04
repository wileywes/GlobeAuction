using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobeAuction.Models
{
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
        
        public bool AttendedEvent { get; set; }

        //children
        public virtual List<AuctionGuest> AuctionGuests { get; set; }
        public virtual List<Student> Students { get; set; }
    }

    public class AuctionGuest
    {
        public int AuctionGuestId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TicketType { get; set; }
        public decimal TicketPrice { get; set; }

        public virtual Bidder Bidder { get; set; }
    }

    public class TicketPurchase
    {
        public int TicketPurchaseId { get; set; }
        public string TicketType { get; set; }
        public decimal TicketPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal? TicketPricePaid { get; set; }

        public bool IsTicketPaid { get { return TicketPricePaid.HasValue; } }

        public virtual AuctionGuest AuctionGuest { get; set; }

        [Required]  //<======= Forces Cascade delete
        public Invoice Invoice { get; set; }
    }

    public class Student
    {
        public int StudentId { get; set; }
        public string HomeroomTeacher { get; set; }

        public virtual Bidder Bidder { get; set; }
    }
}