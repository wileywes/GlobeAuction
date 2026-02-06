using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using GlobeAuction;
using TechTalk.SpecFlow.Assist;
using GlobeAuction.Controllers;
using GlobeAuction.Models;
using System.Web.Mvc;
using GlobeAuction.Helpers;

namespace GlobeAuction.Specflow
{
    [Binding]
    public class ItemSteps : BaseSteps
    {
        public ItemSteps(ItemsContext itemsContext)
            : base(itemsContext)
        {
        }

        [Given(@"I create these auction categories")]
        public void GivenICreateTheseAuctionCategories(Table table)
        {
            var catsToCreate = table.CreateSet<AuctionCategory>();
            var catController = new AuctionCategoriesController();
            foreach(var cat in catsToCreate)
            {
                catController.Create(cat);
                ItemsContext.AuctionCategoriesCreated.Add(cat);
            }
        }

        [Given(@"I create these donation items in category '(.*)'")]
        public void GivenICreateTheseDonationItems(string categoryName, Table table)
        {
            var itemsToCreate = table.CreateSet<DonationItem>();
            var diController = new DonationItemsController();

            var categoryId = new ItemsRepository(_db).GetCatalogData().Categories.First(c => c.Name.Equals(categoryName)).AuctionCategoryId;

            foreach (var item in itemsToCreate)
            {
                //fake the rest of the items
                item.Solicitor = new Solicitor
                {
                    Email = "specflowtest@gmail.com",
                    FirstName = "Specflow",
                    LastName = "Test",
                    Phone = "123-123-1234"
                };
                item.Donor = new Donor
                {
                    Email = "specflowdonor@gmail.com",
                    Address1 = "123 Test Dr",
                    City = "Atlanta",
                    State = "GA",
                    Zip = "30319",
                    BusinessName = "Donor Business",
                    ContactName = "Specflow Contact",
                    Phone = "234-234-2345"
                };

                diController.Create(item, "1", categoryId.ToString(), null);
                ItemsContext.DonationItemsCreated.Add(item);
            }
        }

        [Then(@"the donation items in the category '(.*)' are")]
        public void ThenTheDonationItemsInTheCategoryAre(string categoryName, Table expected)
        {
            var actuals = ((IEnumerable<DonationItem>)(DonationItemsController.Index() as ViewResult).Model).ToList();
            var itemsInRequestedCategory = actuals.Where(i => i.Category.Name.Equals(categoryName)).ToList();
            expected.CompareToSet(itemsInRequestedCategory);
        }

        [When(@"I convert the created donation items to single auction items")]
        public void WhenIConvertTheCreatedDonationItemsToSingleAuctionItems()
        {
            var donationItemIds = string.Join(",", ItemsContext.DonationItemsCreated.Select(d => d.DonationItemId));
            AuctionItemsController.SubmitSelectedDonationItems("MakeSingle", donationItemIds, null, null);
        }

        [Then(@"the auction items in the category '(.*)' are")]
        public void ThenTheAuctionItemsInTheCategoryAre(string categoryName, Table expected)
        {
            var model = (ItemsViewModel)(AuctionItemsController.Index() as ViewResult).Model;
            var itemsInRequestedCategory = model.AuctionItems.Where(i => i.Category.Equals(categoryName)).ToList();
            expected.CompareToSet(itemsInRequestedCategory);
        }
    }
}
