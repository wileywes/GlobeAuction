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
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string Restrictions { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [DataType(DataType.Currency)]
        public int? DollarValue { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool IsDeleted { get; set; }
        
        public Solicitor Solicitor { get; set; }
        public Donor Donor { get; set; }

        public DonationItem Clone()
        {
            var newItem = (DonationItem)MemberwiseClone();
            newItem.DonationItemId = 0;
            newItem.Donor = this.Donor;
            newItem.Solicitor = this.Solicitor;
            return newItem;
        }
    }
}