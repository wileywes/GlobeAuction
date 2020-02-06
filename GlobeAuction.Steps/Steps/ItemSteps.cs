using GlobeAuction.Steps.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using GlobeAuction;

namespace GlobeAuction.Steps.Steps
{
    public class ItemSteps
    {
        private ItemsContext _itemsContext;

        public ItemSteps(ItemsContext itemsContext)
        {
            _itemsContext = itemsContext;
        }

        [Given(@"I create these auction items")]
        public void GivenICreateTheseAuctionItems(Table table)
        {
            var aiController = new GlobeAuction.Controllers.AuctionItemsController;
        }
    }
}
