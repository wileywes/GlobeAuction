using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GlobeAuction.Models
{
    public class InvoiceLookupModel
    {
        [Required]
        [Display(Name = "Paddle No.")]
        public int? BidderId { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }

    public class InvoicesForBidderViewModel
    {
        public int BidderId { get; set; }
        public string BidderName { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string BidderPhone { get; set; }
        [DataType(DataType.EmailAddress)]
        public string BidderEmail { get; set; }
        [DataType(DataType.PostalCode)]
        public string BidderZipCode { get; set; }

        public List<Invoice> Invoices { get; set; }

        public InvoicesForBidderViewModel(Bidder bidder, IEnumerable<Invoice> invoices)
        {
            BidderId = bidder.BidderId;
            BidderName = bidder.FirstName + "  " + bidder.LastName;
            BidderPhone = bidder.Phone;
            BidderEmail = bidder.Email;
            BidderZipCode = bidder.ZipCode;

            Invoices = (invoices ?? new List<Invoice>()).ToList();
        }
    }
}