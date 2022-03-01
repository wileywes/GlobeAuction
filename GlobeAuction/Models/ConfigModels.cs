using System;
using System.ComponentModel.DataAnnotations;

namespace GlobeAuction.Models
{
    public class ConfigProperty
    {
        [Key]
        public int ConfigPropertyId { get; set; }

        [Required]
        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }
}