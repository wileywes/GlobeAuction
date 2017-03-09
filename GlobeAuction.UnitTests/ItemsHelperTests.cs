using GlobeAuction.Helpers;
using GlobeAuction.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace GlobeAuction.UnitTests
{
    [TestFixture]
    public class ItemsHelperTests
    {
        [Test]
        public void CreateAuctionItemForDonation_SingleItem()
        {
            var item = new DonationItem
            {
                DollarValue = 98,
                Category = "Test Cat",
                Title = "test Title",
                Description = "test desc"
            };

            var auctionItem = ItemsHelper.CreateAuctionItemForDonation(1, item, "wes");

            Assert.That(auctionItem.UniqueItemNumber, Is.EqualTo(1));
            Assert.That(auctionItem.Title, Is.EqualTo(item.Title));
            Assert.That(auctionItem.Description, Is.EqualTo(item.Description));
            Assert.That(auctionItem.StartingBid, Is.EqualTo(40));
            Assert.That(auctionItem.BidIncrement, Is.EqualTo(5));
            Assert.That(auctionItem.Category, Is.EqualTo(item.Category));
        }

        [Test]
        public void CreateAuctionItemForDonations_MultipleItemsStartingBid()
        {
            var items = new List<DonationItem>
            {
                new DonationItem
                {
                    DollarValue = 85,
                    Category = "Test Cat",
                    Title = "test Title 1",
                    Description = "test desc"
                },
                new DonationItem
                {
                    DollarValue = 50,
                    Category = "Test Cat",
                    Title = "test Title 2",
                    Description = "test desc"
                },
                new DonationItem
                {
                    DollarValue = 118,
                    Category = "Test Cat",
                    Title = "test Title 3",
                    Description = "test desc"
                },
                new DonationItem
                {
                    DollarValue = 128,
                    Category = "Test Cat",
                    Title = "test Title 2",
                    Description = "test desc"
                },
                new DonationItem
                {
                    DollarValue = 112,
                    Category = "Test Cat",
                    Title = "test Title 3",
                    Description = "test desc"
                }
            };

            var auctionItem = ItemsHelper.CreateAuctionItemForDonations(1, items, "wes");

            Assert.That(auctionItem.UniqueItemNumber, Is.EqualTo(1));
            Assert.That(auctionItem.StartingBid, Is.EqualTo(195));
            Assert.That(auctionItem.BidIncrement, Is.EqualTo(20));
        }

        [Test]
        public void CreateAuctionItemForDonations_MultipleItemsStartingBid_FitnessAroundAtlanta()
        {
            var items = new List<DonationItem>
            {
                new DonationItem
                {
                    DollarValue = 140,
                    Category = "Test Cat",
                    Title = "test Title 1",
                    Description = "test desc"
                },
                new DonationItem
                {
                    DollarValue = 35,
                    Category = "Test Cat",
                    Title = "test Title 2",
                    Description = "test desc"
                },
                new DonationItem
                {
                    DollarValue = 90,
                    Category = "Test Cat",
                    Title = "test Title 3",
                    Description = "test desc"
                }
            };

            var auctionItem = ItemsHelper.CreateAuctionItemForDonations(1, items, "wes");

            Assert.That(auctionItem.UniqueItemNumber, Is.EqualTo(1));
            Assert.That(auctionItem.StartingBid, Is.EqualTo(105));
            Assert.That(auctionItem.BidIncrement, Is.EqualTo(20));
        }
    }
}
