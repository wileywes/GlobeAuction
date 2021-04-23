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
            "Blade/Craft - 2nd",
            "Brantley - 8th",
            "Bruen - 8th",
            "Clayton-Purvis - 6th",
            "Cooper - 3rd",
            "Datta - 1st",
            "Eckmann - 4th",
            "Fite - K",
            "Flynn - 7th",
            "Godicheau - 3rd",
            "Gonzalez - K",
            "Gordon - 1st",
            "Hardnett - 4th",
            "Hassan - K",
            "Hertz - 8th",
            "Hogan - 2nd",
            "Holmes - 2nd",
            "Horton - 6th",
            "Jiang - 5th",
            "Lalonde - 5th",
            "Lane - K",
            "Libowsky - 7th",
            "Lopez - 5th",
            "Lovejoy - 4th",
            "Marin Valencia - 4th",
            "Marks - 8th",
            "McHargue - 3rd",
            "McKinney - 7th",
            "Morris - 5th",
            "Newman - 6th",
            "Nimene - 1st",
            "Paulsen-Faria - 3rd",
            "Rodgers - 1st",
            "Roe - 4th",
            "Sanders - 4th",
            "Schwarzmer - 6th",
            "Shuai - 3rd",
            "Smith - 7th",
            "Tarazona - 2nd",
            "Tendon - 2nd",
            "Teufel - 5th",
            "Tolbert  - 6th",
            "Varino - 1st",
            "Wang - 1st",
            "Wang - 2nd",
            "Wang - K",
            "West - 3rd",
            "Whitelegg - 5th"
        };
    }
}