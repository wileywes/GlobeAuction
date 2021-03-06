﻿using System;
using System.ComponentModel.DataAnnotations;

namespace GlobeAuction.Models
{
    public class TicketType
    {
        public int TicketTypeId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        public bool OnlyVisibleToAdmins { get; set; }
        public string PromoCode { get; set; }
        
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }
}