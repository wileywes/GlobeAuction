using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GlobeAuction.Models
{
    public class Solicitor
    {
        public int SolicitorId { get; set; }

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

        public List<DonationItem> DonationItems { get; set; }
    }
}