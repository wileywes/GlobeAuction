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
            "Fund-a-Project",
            "Food Donations"
        };

        public static readonly List<string> TeacherNames = new List<string>
        {
            "N/A",
        "Barreneche - K",
        "Hogan - K",
        "Liu - K",
        "Holmes - K",
        "Tendon - K",
        "Lambiotte - K",
        "Romero - 1",
        "Comstock - 1",
        "Wang - 1",
        "Johnson - 1",
        "Hassan - 1",
        "Mitchell - 1",
        "Bukhaya - 2",
        "Sanders - 2",
        "Wang - 2",
        "Blade - 2",
        "Godicheaux - 2",
        "McCafferty - 2",
        "Juhan - 3",
        "Cooper - 3",
        "Shuai - 3",
        "West - 3",
        "Marin - 4",
        "Eckmann - 4",
        "Roe - 4",
        "Morgan - 4",
        "Marsh - 4",
        "Roe - 5",
        "Lopez - 5",
        "Freedman - 5",
        "Surrett - 5",
        "White - 5",
        "Letts - 6",
        "Gray - 6",
        "Newman - 6",
        "Schwarzmer - 6",
        "Clayton-Purvis - 6",
        "Libowsky - 7",
        "Flynn - 7",
        "Morris - 7",
        "Smith - 7",
        };
    }
}