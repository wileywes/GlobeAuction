using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeAuction.Steps.Context
{
    public class ItemsContext
    {
        public List<AuctionCategory> AuctionCategoriesCreated { get; set; }
        public List<DonationItem> DonationItemsCreated { get; set; }

        public ItemsContext()
        {
            AuctionCategoriesCreated = new List<AuctionCategory>();
            DonationItemsCreated = new List<DonationItem>();
        }
    }
}
