using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobeAuction.Helpers
{
    public class ItemsHelper
    {
        public static AuctionItem CreateAuctionItemForDonation(int uniqueId, DonationItem item, string username)
        {
            return CreateAuctionItemForDonations(uniqueId, new List <DonationItem> { item }, username);
        }

        public static AuctionItem CreateAuctionItemForDonations(int uniqueId, List<DonationItem> items, string username)
        {
            if (!items.Any()) throw new ApplicationException("You must select at least one donation item");

            var totalDollarValue = items.Sum(i => i.DollarValue.GetValueOrDefault(0));
            var startBid = Math.Floor(totalDollarValue * 0.4);
            startBid = Math.Round(startBid / 5.0) * 5;
            var mostCommonCategory = items.GroupBy(i => i.Category)
                .OrderByDescending(g => g.Count())
                .First().Key;

            int bidIncrement;
            if (startBid < 50) bidIncrement = 5;
            else if (startBid <= 100) bidIncrement = 10;
            else bidIncrement = 20;

            var description = items.Count == 1 ? items.First().Description :
                "This basket includes:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, items.Select(i => i.Description));

            var title = items.First().Title;

            return new AuctionItem
            {
                UniqueItemNumber = uniqueId,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UpdateBy = username,
                StartingBid = (int)startBid,
                BidIncrement = bidIncrement,
                Category = mostCommonCategory,
                Description = description,
                Title = title,
                DonationItems = items
            };
        }

        public static string GetItemNameForPayPalCart(AuctionItem item)
        {
            return $"Auction Item #{item.UniqueItemNumber}: {item.Title}";
        }

        public static void GetAuctionItemInfoFromPayPalCartItemName(string payPalCartItemName, out int uniqueItemNumber, out string titlePart)
        {
            var noPrefix = payPalCartItemName.Replace("Auction Item #", "");
            var parts = noPrefix.Split(':');
            uniqueItemNumber = int.Parse(parts[0]);
            titlePart = parts[1].TrimStart();
        }
    }
}