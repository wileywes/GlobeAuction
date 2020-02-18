using GlobeAuction.Controllers;
using GlobeAuction.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace GlobeAuction.Specflow
{
    [Binding]
    public class CommonSteps : BaseSteps
    {
        public CommonSteps(ItemsContext itemsContext)
            : base(itemsContext)
        {
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            AutoMapperConfig.RegisterMappings();
            EmailHelperFactory.UseNoOpEmailHelper();
        }

        [AfterScenario("hook_purgeall")]
        public void AfterScenarioPurgeAll()
        {
            //delete all auction items
            foreach(var itemId in _db.AuctionItems.Select(ai => ai.AuctionItemId))
            {
                AuctionItemsController.DeleteConfirmed(itemId);
            }

            foreach (var itemId in _db.DonationItems.Select(ai => ai.DonationItemId))
            {
                DonationItemsController.DeleteConfirmed(itemId);
            }

            //now hard delete donation items
            _db.DonationItems.RemoveRange(_db.DonationItems);
            _db.SaveChanges();

            //delete auction categories
            foreach (var cat in ItemsContext.AuctionCategoriesCreated)
            {
                AuctionCategoriesController.DeleteConfirmed(cat.AuctionCategoryId);
            }

            //delete bidders
            foreach(var bidderId in _db.Bidders.Select(b => b.BidderId))
            {
                //soft delete
                BiddersController.DeleteConfirmed(bidderId);
            }

            //now hard delete bidders
            _db.Students.RemoveRange(_db.Students);
            _db.AuctionGuests.RemoveRange(_db.AuctionGuests);
            _db.Bidders.RemoveRange(_db.Bidders);
            _db.SaveChanges();
        }
    }
}
