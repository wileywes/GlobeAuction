﻿using AutoMapper;
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

            //calculate the starting bid
            var totalDollarValue = items.Sum(i => i.DollarValue.GetValueOrDefault(0)); //value of all donation items in basket
            var startBid = Math.Floor(totalDollarValue * 0.4); // start at 40% of value
            startBid = Math.Round(startBid / 5.0) * 5; //round to nearest $5 increment

            //calculate the bid increment based on ranges of the starting bid
            int bidIncrement;
            if (startBid < 50) bidIncrement = 5;
            else if (startBid <= 100) bidIncrement = 10;
            else bidIncrement = 20;

            //generate the basket description based on the items in it
            var description = items.Count == 1 ? items.First().Description :
                "This basket includes:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, items.Select(i => i.Description));

            //basket title comes from the first donation item
            var title = items.First().Title;

            var mostCommonCategory = items.GroupBy(i => i.Category)
                .OrderByDescending(g => g.Count())
                .First().Key;

            return new AuctionItem
            {
                UniqueItemNumber = uniqueId,
                CreateDate = Utilities.GetEasternTimeNow(),
                UpdateDate = Utilities.GetEasternTimeNow(),
                UpdateBy = username,
                StartingBid = (int)startBid,
                BidIncrement = bidIncrement,
                Category = mostCommonCategory,
                Description = description,
                Title = title,
                DonationItems = items,
                Quantity = items.Count == 1 ? items.First().Quantity : 1
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

        public AuctionItem GetItemWithAllBidInfo(int itemNo)
        {
            return db.AuctionItems
                .Include(a => a.AllBids)
                .Include("AllBids.Bidder")
                .FirstOrDefault(a => a.UniqueItemNumber == itemNo);
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

            biddersThatLost = new List<Bidder>();

            //recalculate winners
            var index = 0;
            foreach(var bid in item.AllBids.OrderByDescending(b => b.BidAmount))
            {
                var nowWinning = index < item.Quantity;
                if (bid.IsWinning != nowWinning)
                {
                    //changing
                    bid.UpdateBy = bidder.Email;
                    bid.UpdateDate = time;
                }
                //lost win
                if (bid.IsWinning && !nowWinning)
                {
                    //send text to this bidder
                    biddersThatLost.Add(bid.Bidder);
                }

                bid.IsWinning = nowWinning;
                index++;
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
        public List<Bid> Winnings { get; set; }
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