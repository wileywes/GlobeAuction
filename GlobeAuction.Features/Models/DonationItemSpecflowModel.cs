using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobeAuction.Steps.Models
{
    public class DonationItemSpecflowModel : DonationItem
    {
        public int CopiesToCreate { get; set; }
        public string CategoryName { get; set; }
    }
}
