using GlobeAuction.Steps.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using GlobeAuction;
using TechTalk.SpecFlow.Assist;
using GlobeAuction.Steps.Models;
using GlobeAuction.Controllers;
using System.Web.Mvc;
using GlobeAuction.Models;

namespace GlobeAuction.Steps.Steps
{
    [Binding]
    public class ItemSteps
    {
        private ItemsContext _itemsContext;

        public ItemSteps(ItemsContext itemsContext)
        {
            _itemsContext = itemsContext;
        }

        [Given(@"I create these donation items")]
        public void GivenICreateTheseDonationItems(Table table)
        {
            var itemsToCreate = table.CreateSet<DonationItemSpecflowModel>();
            var diController = new DonationItemsController();
            foreach(var item in itemsToCreate)
            {
                diController.Create(item, item.Qty.ToString(), item.CategoryName);
                _itemsContext.DonationItemsCreated.Add(item);
            }
        }

        [Then(@"the donation items are")]
        public void ThenTheDonationItesAre(Table expected)
        {
            var actuals = (IEnumerable<DonationItem>)(new DonationItemsController().Index() as ViewResult).Model;
            expected.CompareToSet(actuals);
        }
    }
}
