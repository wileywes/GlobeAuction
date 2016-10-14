using System;
using System.ComponentModel.DataAnnotations;

namespace GlobeAuction.Models
{
    public class DonationItem
    {
        public int DonationItemId { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Description { get; set; }

        public string Restrictions { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [DataType(DataType.Currency)]
        public int? DollarValue { get; set; }

        //solicitor can enter quantity and we'll create separate DonationItems for each one

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        
        public Solicitor Solicitor { get; set; }
        public Donor Donor { get; set; }
    }
}