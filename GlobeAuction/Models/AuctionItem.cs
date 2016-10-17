using System;
using System.Collections.Generic;

namespace GlobeAuction.Models
{
    public class AuctionItem
    {
        /// <summary>
        /// Entered by admin, uniqueness enforced by DB
        /// </summary>
        public int UniqueItemNumber { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int StartingBid { get; set; }
        public int BidIncrement { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        
        /// <summary>
        /// List of items in this buyable group - could be 1 or more donation items
        /// </summary>
        public List<DonationItem> DonationItems { get; set; }
        
        public int WinningBidderId { get; set; }
        public int WinningBid { get; set; }
    }
}