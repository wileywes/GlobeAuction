using GlobeAuction.Steps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeAuction.Steps.Context
{
    public class ItemsContext
    {
        public List<DonationItemSpecflowModel> DonationItemsCreated { get; set; }

        public ItemsContext()
        {
            DonationItemsCreated = new List<DonationItemSpecflowModel>();
        }
    }
}
