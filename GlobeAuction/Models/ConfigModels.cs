using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GlobeAuction.Models
{
    public class ConfigProperty
    {
        [Key]
        public int ConfigPropertyId { get; set; }

        [Required]
        public string PropertyName { get; set; }

        [AllowHtml]
        public string PropertyValue { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }

    public class Sponsor
    {
        public int SponsorId { get; set; }

        [Required]
        public string Level { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }

    public class SponsorsViewModel
    {
        public Dictionary<string, List<Sponsor>> SponsorsByLevel { get; set; }
    }
}