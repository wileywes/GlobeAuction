using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GlobeAuction.Models
{
    public class Faq
    {
        [Key]
        public int FaqId { get; set; }

        [Required]
        [AllowHtml]
        public string Question { get; set; }

        [AllowHtml]
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

    public class FaqsViewModel
    {
        public List<FaqCategoryViewModel> CategoriesWithFAQs { get; set; }
    }

    public class FaqCategoryViewModel
    {
        public FaqCategory Category { get; set; }
        public List<Faq> FAQs { get; set; }
    }
}