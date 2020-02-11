using GlobeAuction.Controllers;
using GlobeAuction.Steps.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace GlobeAuction.Steps
{
    public class CommonSteps
    {
        private ItemsContext _itemsContext;

        public CommonSteps(ItemsContext itemsContext)
        {
            _itemsContext = itemsContext;
        }

        [AfterScenario("hook_purgeall")]
        public void AfterScenarioPurgeAll()
        {
            //delete all donation items
            foreach(var di in _itemsContext.DonationItemsCreated)
            {
                new DonationItemsController().DeleteConfirmed(di.DonationItemId);
            }
        }
    }
}
