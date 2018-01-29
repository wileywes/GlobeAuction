using AutoMapper;
using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace GlobeAuction.Helpers
{
    public class ItemsRepository
    {
        private ApplicationDbContext db;

        public ItemsRepository(ApplicationDbContext context)
        {
            db = context;
        }

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

        public List<WinningsByBidder> GetWinningsByBidder()
        {
            var itemsWon = db.AuctionItems
                .Include(a => a.Invoice)
                .Where(ai => ai.WinningBid.HasValue && ai.WinningBidderId.HasValue)
                .ToList();

            var uniqueBidderIds = itemsWon.Select(i => i.WinningBidderId.Value).Distinct().ToList();
            var bidders = db.Bidders.Where(b => uniqueBidderIds.Contains(b.BidderId)).ToList();

            var results = new List<WinningsByBidder>();
            foreach (var b in bidders)
            {
                var winnings = itemsWon.Where(i => i.WinningBidderId.Value == b.BidderId).ToList();
                var allPaidFor = winnings.All(w => w.Invoice != null && w.Invoice.IsPaid);
                var arePartiallyPaidFor = !allPaidFor && winnings.Any(w => w.Invoice != null && w.Invoice.IsPaid);

                results.Add(new WinningsByBidder
                {
                    AreWinningsAllPaidFor = allPaidFor,
                    AreWinningsPartiallyPaidFor = arePartiallyPaidFor,
                    Bidder = b,
                    Winnings = winnings
                });
            }
            return results;
        }

        public List<StoreItemPurchase> GetStorePurchasesWithIndividualizedRaffleTickets(List<StoreItemPurchaseViewModel> purchaseViewModels)
        {
            var purchasesToReturn = new List<StoreItemPurchase>();

            if (purchaseViewModels != null)
            {
                foreach (var storeItemPurchase in purchaseViewModels.Where(s => s.Quantity > 0))
                {
                    if (storeItemPurchase.StoreItem.IsRaffleTicket)
                    {
                        //if multiple quantity then create individual StoreItemPurchases for each so we have unique IDs
                        for (int i = 0; i < storeItemPurchase.Quantity; i++)
                        {
                            var lineItem = Mapper.Map<StoreItemPurchase>(storeItemPurchase);
                            lineItem.Quantity = 1;
                            purchasesToReturn.Add(lineItem);
                        }
                    }
                    else
                    {
                        var lineItem = Mapper.Map<StoreItemPurchase>(storeItemPurchase);
                        purchasesToReturn.Add(lineItem);
                    }
                }
            }

            //reload StoreItem info from the database so we aren't modifying it (there's got to be a better way to do this)
            foreach(var sip in purchasesToReturn)
            {
                sip.StoreItem = db.StoreItems.Find(sip.StoreItem.StoreItemId);
            }

            return purchasesToReturn;
        }
    }

    public class WinningsByBidder
    {
        public Bidder Bidder { get; set; }
        public List<AuctionItem> Winnings { get; set; }
        public bool AreWinningsAllPaidFor { get; set; }
        public bool AreWinningsPartiallyPaidFor { get; set; }
    }
}