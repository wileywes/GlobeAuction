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
            return CreateAuctionItemForDonations(uniqueId, new List<DonationItem> { item }, username);
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

        public void CreateStoreItemsForDonations(List<DonationItem> items, string username)
        {
            if (!items.Any()) throw new ApplicationException("You must select at least one donation item");

            var donationItemIdsThatAlreadyHaveStoreItems = db.StoreItems
                .Where(si => si.DonationItem != null)
                .ToList() //run the DB query
                .ToDictionary(si => si.DonationItem.DonationItemId, si => si.StoreItemId);

            foreach(var donation in items)
            {
                if (donationItemIdsThatAlreadyHaveStoreItems.ContainsKey(donation.DonationItemId))
                {
                    var storeItemId = donationItemIdsThatAlreadyHaveStoreItems[donation.DonationItemId];
                    var storeItem = db.StoreItems.Find(storeItemId);
                    storeItem.IsDeleted = false;
                }
                else
                {
                    var storeItem = new StoreItem
                    {
                        CanPurchaseInAuctionCheckout = false,
                        CanPurchaseInBidderRegistration = false,
                        CanPurchaseInStore = false,
                        CreateDate = DateTime.Now,
                        Description = donation.Description,
                        IsDeleted = false,
                        IsRaffleTicket = false,
                        OnlyVisibleToAdmins = false,
                        Price = donation.DollarValue.GetValueOrDefault(9999),
                        Quantity = 1,
                        Title = donation.Title,
                        UpdateBy = username,
                        UpdateDate = DateTime.Now
                    };

                    storeItem.DonationItem = donation;
                    db.StoreItems.Add(storeItem);
                }

                donation.IsDeleted = true;
            }
            db.SaveChanges();
        }

        public void MoveStoreItemsBackToDonations(List<StoreItem> items, string username)
        {
            if (!items.Any()) throw new ApplicationException("You must select at least one store item");
            
            foreach (var storeItem in items)
            {
                if (storeItem.DonationItem != null)
                {
                    storeItem.IsDeleted = true;
                    storeItem.DonationItem.IsDeleted = false;
                }
            }
            db.SaveChanges();
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
                .Include(a => a.DonationItems)
                .Where(ai => ai.WinningBid.HasValue && ai.WinningBidderId.HasValue)
                .ToList();

            var uniqueBidderIds = itemsWon.Select(i => i.WinningBidderId.Value).Distinct().ToList();
            var bidders = db.Bidders.Where(b => uniqueBidderIds.Contains(b.BidderId)).ToList();

            var results = new List<WinningsByBidder>();
            foreach (var b in bidders)
            {
                var winnings = itemsWon.Where(i => i.WinningBidderId.Value == b.BidderId).ToList();
                var allPaidFor = winnings.All(w => w.Invoice != null && w.Invoice.IsPaid);
                results.Add(new WinningsByBidder
                {
                    AreWinningsAllPaidFor = allPaidFor,
                    Bidder = b,
                    Winnings = winnings
                });
            }
            return results;
        }

        public List<WinningsByBidder> Mock_GetWinningsByBidder()
        {
            var random = new Random();
            var allItems = db.AuctionItems
                .Include(a => a.Invoice)
                .Include(a => a.DonationItems)
                .ToList();

            var first20Bidders = db.Bidders.Take(20)
                .ToList()
                .Select(b => new WinningsByBidder
                {
                    AreWinningsAllPaidFor = false,
                    Bidder = b,
                    Winnings = new List<AuctionItem>()
                })
                .ToList();

            foreach(var item in allItems)
            {
                //mock winner
                item.WinningBid = random.Next(1, 900);

                var index = random.Next(20);
                first20Bidders[index].Winnings.Add(item);
            }

            return first20Bidders.Where(b => b.Winnings.Count > 0).ToList();
        }

        public List<StoreItemPurchase> GetStorePurchasesWithIndividualizedRaffleTickets(List<BuyItemViewModel> purchaseViewModels)
        {
            var purchasesToReturn = new List<StoreItemPurchase>();

            if (purchaseViewModels != null)
            {
                foreach (var itemPurchase in purchaseViewModels.Where(s => s.QuantityPurchased.GetValueOrDefault(0) > 0))
                {
                    //reload from DB instead of relying on payload posted back to have all the StoreItem details
                    var storeItem = db.StoreItems.Find(itemPurchase.StoreItemId);
                    var storeItemViewModel = Mapper.Map<StoreItemViewModel>(storeItem);

                    if (storeItem.IsRaffleTicket)
                    {
                        if (storeItem.BundleComponents != null && storeItem.BundleComponents.Any())
                        {
                            foreach(var component in storeItem.BundleComponents)
                            {
                                var componentStoreItem = db.StoreItems.Find(component.StoreItemId);
                                var totalQtyForThisTicket = component.Quantity * itemPurchase.QuantityPurchased.Value;

                                //if multiple quantity then create individual StoreItemPurchases for each so we have unique IDs
                                for (int i = 0; i < totalQtyForThisTicket; i++)
                                {
                                    var lineItem = new StoreItemPurchase
                                    {
                                        StoreItem = componentStoreItem,
                                        Quantity = 1,
                                        Price = component.ComponentUnitPrice                                        
                                    };

                                    purchasesToReturn.Add(lineItem);
                                }
                            }
                        }
                        else
                        {
                            //if multiple quantity then create individual StoreItemPurchases for each so we have unique IDs
                            for (int i = 0; i < itemPurchase.QuantityPurchased; i++)
                            {
                                var lineItem = new StoreItemPurchase
                                {
                                    StoreItem = storeItem,
                                    Quantity = 1,
                                    Price = storeItem.Price
                                };
                                purchasesToReturn.Add(lineItem);
                            }
                        }
                    }
                    else
                    {
                        if (storeItem.Quantity <= 0 || storeItem.Quantity < itemPurchase.QuantityPurchased)
                        {
                            throw new OutOfStockException("StoreItem out of stock", storeItem, itemPurchase);
                        }

                        var lineItem = new StoreItemPurchase
                        {
                            StoreItem = storeItem,
                            Quantity = itemPurchase.QuantityPurchased.Value,
                            Price = storeItem.Price
                        };
                        purchasesToReturn.Add(lineItem);
                    }
                }
            }

            foreach (var sip in purchasesToReturn)
            {
                //if item isn't a raffle ticket with unlimited quantity then decrement the available quantity on the store item
                if (sip.StoreItem.IsRaffleTicket == false)
                {
                    sip.StoreItem.Quantity -= sip.Quantity;
                }
            }

            return purchasesToReturn;
        }
    }

    public class WinningsByBidder
    {
        public Bidder Bidder { get; set; }
        public List<AuctionItem> Winnings { get; set; }
        public bool AreWinningsAllPaidFor { get; set; }
    }

    public class OutOfStockException : ApplicationException
    {
        public StoreItem StoreItem { get; private set;}
        public BuyItemViewModel BuyItemViewModel { get; private set; }

        public OutOfStockException(string message, StoreItem storeitem, BuyItemViewModel itemViewModel)
            :base(message)
        {
            StoreItem = storeitem;
            BuyItemViewModel = itemViewModel;
        }
    }
}