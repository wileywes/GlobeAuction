using AutoMapper;
using GlobeAuction.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GlobeAuction.Helpers
{
    public class ItemsRepository
    {
        private static CatalogData _catalogCache;
        private ApplicationDbContext db;

        public ItemsRepository(ApplicationDbContext context)
        {
            db = context;
        }

        public AuctionItem CreateAuctionItemForDonation(DonationItem item, string username)
        {
            return CreateAuctionItemForDonations(new List<DonationItem> { item }, username);
        }

        public AuctionItem CreateAuctionItemForDonations(List<DonationItem> items, string username)
        {
            if (!items.Any()) throw new ApplicationException("You must select at least one donation item");

            int startingBid, bidIncrement;
            CalculateStartBidAndIncrement(items, out startingBid, out bidIncrement);

            //generate the basket description based on the items in it
            var description = items.Count == 1 ? items.First().Description :
                "This basket includes:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, items.Select(i => i.Description));

            //basket title comes from the first donation item
            var title = items.First().Title;

            var mostCommonCategory = items.GroupBy(i => i.Category)
                .OrderByDescending(g => g.Count())
                .First().Key;

            //unique item number comes from category if defined, otherwise we take the max
            var uniqueId = CalculateNextAuctionItemNumber(mostCommonCategory);

            return new AuctionItem
            {
                UniqueItemNumber = uniqueId,
                CreateDate = Utilities.GetEasternTimeNow(),
                UpdateDate = Utilities.GetEasternTimeNow(),
                UpdateBy = username,
                StartingBid = startingBid,
                BidIncrement = bidIncrement,
                IsFixedPrice = false,
                Category = mostCommonCategory,
                Description = description,
                Title = title,
                DonationItems = items,
                Quantity = items.Count == 1 ? items.First().Quantity : 1
            };
        }
        
        public static void CalculateStartBidAndIncrement(List<DonationItem> items, out int startingBid, out int bidIncrement)
        {
            //calculate the starting bid
            var totalDollarValue = items.Sum(i => i.DollarValue.GetValueOrDefault(0)); //value of all donation items in basket
            var startBid = Math.Floor(totalDollarValue * 0.4); // start at 40% of value
            startBid = Math.Round(startBid / 5.0) * 5; //round to nearest $5 increment

            startingBid = (int)startBid;

            //calculate the bid increment based on ranges of the starting bid
            if (startBid < 50) bidIncrement = 5;
            else if (startBid <= 100) bidIncrement = 10;
            else bidIncrement = 20;
        }

        private int CalculateNextAuctionItemNumber(AuctionCategory mostCommonCategory)
        {
            int? uniqueId = null;
            var categoriesWithRanges = db.AuctionCategories.Where(c => c.ItemNumberStart.HasValue && c.ItemNumberEnd.HasValue).ToList();

            if (categoriesWithRanges.Any())
            {
                var catStart = mostCommonCategory?.ItemNumberStart.GetValueOrDefault(0);
                var catEnd = mostCommonCategory?.ItemNumberEnd.GetValueOrDefault(0);
                if (catStart > 0 && catEnd > 0 && catStart < catEnd)
                {
                    var highestInCategory = db.AuctionItems
                        .Where(a => a.Category.AuctionCategoryId == mostCommonCategory.AuctionCategoryId)
                        .OrderByDescending(a => a.UniqueItemNumber)
                        .FirstOrDefault();

                    if (highestInCategory != null)
                    {
                        //take the next one up as long as we aren't at the end
                        if (highestInCategory.UniqueItemNumber < catEnd)
                        {
                            uniqueId = highestInCategory.UniqueItemNumber + 1;
                        }
                    }
                    else
                    {
                        //nothing in category yet so start at the beginning
                        uniqueId = catStart;
                    }

                    //make sure our derived value isn't taken
                    if (uniqueId.HasValue)
                    {
                        var existingAlreadyUsing = db.AuctionItems.FirstOrDefault(a => a.UniqueItemNumber == uniqueId);
                        if (existingAlreadyUsing != null) uniqueId = null;
                    }
                }

                if (!uniqueId.HasValue)
                {
                    //go to the end of the list
                    var topOfCategories = categoriesWithRanges.Max(c => c.ItemNumberEnd.Value) + 1;

                    var topOfItems = 0;
                    if (db.AuctionItems.Any())
                    {
                        topOfItems = db.AuctionItems.Max(a => a.UniqueItemNumber) + 1;
                    }

                    uniqueId = Math.Max(topOfCategories, topOfItems);
                }
            }
            else
            {
                //not using category ranges so just do based on items
                if (db.AuctionItems.Any())
                {
                    uniqueId = db.AuctionItems.Max(a => a.UniqueItemNumber) + 1;
                }
                else
                {
                    uniqueId = 1;
                }
            }

            return uniqueId.Value;
        }

        public void CreateStoreItemsForDonations(List<DonationItem> items, string username)
        {
            if (!items.Any()) throw new ApplicationException("You must select at least one donation item");

            var donationItemIdsThatAlreadyHaveStoreItems = db.StoreItems
                .Where(si => si.DonationItem != null)
                .ToList() //run the DB query
                .ToDictionary(si => si.DonationItem.DonationItemId, si => si.StoreItemId);

            foreach (var donation in items)
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
                        CreateDate = Utilities.GetEasternTimeNow(),
                        Description = donation.Description,
                        IsDeleted = false,
                        IsRaffleTicket = false,
                        OnlyVisibleToAdmins = false,
                        Price = donation.DollarValue.GetValueOrDefault(9999),
                        Quantity = donation.Quantity,
                        Title = donation.Title,
                        UpdateBy = username,
                        UpdateDate = Utilities.GetEasternTimeNow()
                    };

                    storeItem.DonationItem = donation;
                    db.StoreItems.Add(storeItem);
                }

                donation.IsDeleted = true;
                donation.IsInStore = true;
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
                    storeItem.DonationItem.IsInStore = false;
                }
            }
            db.SaveChanges();
        }

        public void EndRafflePurchasing()
        {
            foreach (var storeItem in db.StoreItems.Where(si => si.IsRaffleTicket))
            {
                storeItem.CanPurchaseInAuctionCheckout =
                    storeItem.CanPurchaseInBidderRegistration =
                    storeItem.CanPurchaseInStore = false;
            }
            db.SaveChanges();
        }

        public AuctionItem GetItemWithAllBidInfo(int itemNo)
        {
            return db.AuctionItems
                .Include(a => a.AllBids)
                .Include(a => a.DonationItems)
                .Include("AllBids.Bidder")
                .FirstOrDefault(a => a.UniqueItemNumber == itemNo);
        }

        public void DeleteBidAndRecalcWinners(AuctionItem item, Bid bidToRemove)
        {
            db.Bids.Remove(bidToRemove);

            List<Bidder> biddersThatLost;
            RecalculateWinnersAndSave(item, null, HttpContext.Current.User.Identity.Name, out biddersThatLost);
        }

        public void EnterNewBidAndRecalcWinners(AuctionItem item, Bidder bidder, decimal amount, out List<Bidder> biddersThatLost)
        {
            var time = Utilities.GetEasternTimeNow();
            var newBid = new Bid
            {
                AuctionItem = item,
                Bidder = bidder,
                BidAmount = amount,
                CreateDate = time,
                UpdateBy = bidder.Email,
                UpdateDate = time
            };
            item.AllBids.Add(newBid);

            RecalculateWinnersAndSave(item, bidder.BidderId, bidder.Email, out biddersThatLost);
        }

        private void RecalculateWinnersAndSave(AuctionItem item, int? bidderIdEnteringBid, string updatedByEmail, out List<Bidder> biddersThatLost)
        {
            var time = Utilities.GetEasternTimeNow();
            biddersThatLost = new List<Bidder>();

            //recalculate winners
            var index = 0;
            foreach (var bid in item.AllBids.OrderByDescending(b => b.BidAmount))
            {
                var nowWinning = index < item.Quantity;
                if (bid.IsWinning != nowWinning)
                {
                    //changing
                    bid.UpdateBy = updatedByEmail;
                    bid.UpdateDate = time;
                }
                //lost win
                if (bid.IsWinning && !nowWinning && bidderIdEnteringBid.HasValue && bid.Bidder.BidderId != bidderIdEnteringBid.Value)
                {
                    //send text to this bidder as long as it's not the bidder that just entered the new bid
                    biddersThatLost.Add(bid.Bidder);
                }

                bid.IsWinning = nowWinning;
                index++;
            }
            db.SaveChanges();

            if (item.AllBids.Any())
            {
                var highestBid = item.AllBids.Select(b => b.BidAmount).Max();
                UpdateHighestBidForCachedItem(item.AuctionItemId, (int)highestBid);
            }
            else
            {
                UpdateHighestBidForCachedItem(item.AuctionItemId, item.StartingBid);
            }
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
            var biddersWithWinnings = db.Bidders
                .Include(a => a.Bids)
                .Include("Bids.AuctionItem")
                .Include("Bids.Invoice")
                .Where(b => b.Bids.Any(bid => bid.IsWinning))
                .ToList();

            var results = new List<WinningsByBidder>();
            foreach (var b in biddersWithWinnings)
            {
                var winningBids = b.Bids.Where(bid => bid.IsWinning).ToList();
                var allPaidFor = winningBids.All(w => w.Invoice != null && w.Invoice.IsPaid);
                results.Add(new WinningsByBidder
                {
                    AreWinningsAllPaidFor = allPaidFor,
                    Bidder = b,
                    Winnings = winningBids
                });
            }
            return results;
        }

        public List<WinningsByBidder> Mock_GetWinningsByBidder()
        {
            /*
            var random = new Random();
            var allItems = db.AuctionItems
                .Include(a => a.Invoice)
                .Include(a => a.DonationItems)
                .ToList();

            var first20Bidders = db.Bidders
                .Where(b => b.IsDeleted == false)
                .Take(20)
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

            return first20Bidders.Where(b => b.Winnings.Count > 0).ToList();*/
            throw new NotImplementedException("Need to revisit");
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
                            foreach (var component in storeItem.BundleComponents)
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
                        if (storeItem.HasUnlimitedQuantity == false && (storeItem.Quantity <= 0 || storeItem.Quantity < itemPurchase.QuantityPurchased))
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
                //if item isn't a raffle ticket or a store item with unlimited quantity then decrement the available quantity on the store item
                if (sip.StoreItem.IsRaffleTicket == false && !sip.StoreItem.HasUnlimitedQuantity)
                {
                    sip.StoreItem.Quantity -= sip.Quantity;
                }
            }

            return purchasesToReturn;
        }

        public CatalogData GetCatalogData()
        {
            if (_catalogCache != null)
            {
                return _catalogCache;
            }

            var catData = new CatalogData();
            var items = db.AuctionItems
                .Include(i => i.AllBids)
                .Include(i => i.Category)
                .ToList();
            catData.AuctionItems = items.Select(i => new CatalogAuctionItemViewModel(i)).ToList();

            //categories with items
            var categories = db.AuctionItems.GroupBy(i => i.Category).Select(g => new { Category = g.Key, Count = g.Count() }).ToList();
            catData.Categories = categories.Select(c => new CatalogCategoryViewModel(c.Category, c.Count)).ToList();

            //categories without items
            var allCategories = db.AuctionCategories.ToList();
            foreach (var cat in allCategories)
            {
                if (catData.Categories.FirstOrDefault(c => c.AuctionCategoryId == cat.AuctionCategoryId) == null)
                {
                    catData.Categories.Add(new CatalogCategoryViewModel(cat, 0));
                }
            }

            catData.Categories = catData.Categories.OrderBy(c => c.Name).ToList();

            _catalogCache = catData;
            var log = LogManager.GetCurrentClassLogger();
            log.Info("Loaded catalog data from DB on " + Environment.MachineName);

            return catData;
        }

        public void UpdateHighestBidForCachedItem(int auctionId, int newHighestBid)
        {
            if (_catalogCache != null)
            {
                _catalogCache.UpdateHighestBidForAuctionId(auctionId, newHighestBid);
            }
        }

        public void ClearCatalogDataCache()
        {
            _catalogCache = null;
        }

        public DateTime? GetBiddingEndDateIfCategoriesAreOpen()
        {
            var openCategories = GetCatalogData().Categories.Where(c => c.IsAvailableForMobileBidding && c.IsBiddingOpen).ToList();
            if (openCategories.Any())
            {
                return openCategories.Select(c => c.BidCloseDateLtz).OrderByDescending(c => c).First();
            }

            return null;
        }
    }

    public class WinningsByBidder
    {
        public Bidder Bidder { get; set; }
        public List<Bid> Winnings { get; set; }
        public bool AreWinningsAllPaidFor { get; set; }

        public bool HasPhysicalWinnings
        {
            get
            {
                return Winnings.Any(w => w.AuctionItem.DonationItems.Any(d => !d.UseDigitalCertificateForWinner));
            }
        }
    }

    public class OutOfStockException : ApplicationException
    {
        public StoreItem StoreItem { get; private set; }
        public BuyItemViewModel BuyItemViewModel { get; private set; }

        public OutOfStockException(string message, StoreItem storeitem, BuyItemViewModel itemViewModel)
            : base(message)
        {
            StoreItem = storeitem;
            BuyItemViewModel = itemViewModel;
        }
    }
}