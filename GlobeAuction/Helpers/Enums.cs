using System.Collections.Generic;

namespace GlobeAuction.Models
{
    public class AuctionRoles
    {
        public const string CanEditItems = "canEditItems";
        public const string CanEditWinners = "canEditWinners";
        public const string CanCheckoutWinners = "canCheckoutWinners";
        public const string CanAdminUsers = "canAdminUsers";
        public const string CanEditBidders = "canEditBidders";
        public const string CanEditTickets = "canEditTickets";

        internal static List<string> GetAllRoleNames()
        {
            return new List<string>
            {
                CanAdminUsers,
                CanCheckoutWinners,
                CanEditItems,
                CanEditWinners,
                CanEditBidders,
                CanEditTickets

            };
        }
    }

    public class AuctionConstants
    {
        public static readonly string FundaProjectCategoryName = "Fund-a-Project";

        /*
         * ALSO "Live"
        public static readonly List<string> DonationItemCategories = new List<string>
        {
            "Gift Card Grab",
            "Experiences and Entertainment",
            "Getaways",
            "Health, Beauty and Fitness",
            "Children's Corner",
            "Home Goods and Services",
            "Teacher Treasure",
            "GLOBE Perks",
            "Class Art",
            FundaProjectCategoryName,
            "Food Donations"
        };*/
    }
}