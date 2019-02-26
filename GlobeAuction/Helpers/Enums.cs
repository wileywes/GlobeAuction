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

        public static readonly List<string> TeacherNames = new List<string>
        {
            "N/A",
            "Barreneche - K",
            "Hogan - K",
            "Liu - K",
            "Lane - K",
            "Tendon - K",
            "Lambiotte - K",
            "Tarazona - 1",
            "Comstock - 1",
            "Wang - 1",
            "Johnson - 1",
            "Hassan - 1",
            "Rodgers - 1",
            "Bukhaya - 2",
            "Sanders - 2",
            "Wang - 2",
            "Blade - 2",
            "Godicheau - 2",
            "Holmes - 2",
            "Juhan - 3",
            "Cooper - 3",
            "Shuai - 3",
            "West - 3",
            "Bingham - 3",
            "McCafferty - 3",
            "Marin - 4",
            "Freedman - 4",
            "Eckmann - 4",
            "Marsh - 4",
            "Roe - 5",
            "Lopez - 5",
            "White - 5",
            "Morgan - 5",
            "Jiang - 5",
            "Wang - 5",
            "Tolbert - 6",
            "Gray - 6",
            "Newman - 6",
            "Schwarzmer - 6",
            "Clayton-Purvis - 6",
            "Libowsky - 7",
            "Flynn - 7",
            "McKinney - 7",
            "Smith - 7",
            "Calvo - 8",
            "Morris - 8",
            "Hertz - 8",
            "Brown - 8",
        };
    }
}