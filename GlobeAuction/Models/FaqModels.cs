using System;
using System.ComponentModel.DataAnnotations;

namespace GlobeAuction.Models
{
    public class Faq
    {
        [Key]
        public int FaqId { get; set; }

        [Required]
        public string Question { get; set; }

        public string Answer { get; set; }

        [Required]
        public FaqCategory Category { get; set; }
        
        [Required]
        public int OrderInCategory { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }

    public class FaqCategory
    {
        [Key]
        public int FaqCategoryId { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public int DisplayOrder { get; set; }
    }
}