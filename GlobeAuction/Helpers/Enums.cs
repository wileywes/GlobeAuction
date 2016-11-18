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
        public static readonly List<string> TeacherNames = new List<string>
        {
        "Tendon - K",
        "Lambiotte - K",
        "Hogan - K",
        "Barreneche - K",
        "Eckmann - K",
        "Holmes - K",
        "Hassan - 1",
        "Mitchell - 1",
        "Johnson - 1",
        "Wang - 1",
        "Hemingway - 1",
        "Murphy - 1",
        "Bukhaya - 2",
        "Sanders - 2",
        "Wang - 2",
        "Blade - 2",
        "Daniel - 3",
        "Cooper - 3",
        "Shuai - 3",
        "West - 3",
        "Morgan - 3",
        "Mann - 4",
        "Molla - 4",
        "Marsh - 4",
        "Marin - 4",
        "Libowsky - 4",
        "Morris - 5",
        "Whitfield - 5",
        "White - 5",
        "Schwarzmer - 5",
        "Letts - 6",
        "Gray - 6",
        "Newman - 6"
        };
    }
}