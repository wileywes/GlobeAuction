﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
                if (StoreItemPurchases != null && StoreItemPurchases.Any()) total += StoreItemPurchases.Sum(sip => sip.PricePaid.GetValueOrDefault(0));
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
}