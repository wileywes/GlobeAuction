using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
            var startBid = (int)Math.Floor(totalDollarValue * 0.4);
            var mostCommonCategory = items.GroupBy(i => i.Category)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var description = items.Count == 1 ? items.First().Description :
                "This basket includes:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, items.Select(i => i.Description));

            return new AuctionItem
            {
                UniqueItemNumber = uniqueId,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UpdateBy = username,
                StartingBid = startBid,
                BidIncrement = (int)Math.Floor((decimal)startBid / 4),
                Category = mostCommonCategory,
                Description = description,
                DonationItems = items
            };
        }
    }
}