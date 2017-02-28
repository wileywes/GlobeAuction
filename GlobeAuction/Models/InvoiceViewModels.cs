using System;
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
        public string ZipCode { get; set; }
    }
}