using System.Collections.Generic;

namespace GlobeAuction.Models
{
    public class AuctionRoles
    {
        public const string CanEditItems = "canEditItems";
        public const string CanEditWinners = "canEditWinners";
        public const string CanCheckoutWinners = "canCheckoutWinners";
        public const string CanAdminUsers = "canAdminUsers";

        internal static List<string> GetAllRoleNames()
        {
            return new List<string>
            {
                AuctionRoles.CanAdminUsers,
                AuctionRoles.CanCheckoutWinners,
                AuctionRoles.CanEditItems,
                AuctionRoles.CanEditWinners
            };
        }
    }
}