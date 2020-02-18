using GlobeAuction.Helpers;
using GlobeAuction.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GlobeAuction.Specflow
{
    public class BidSteps : BaseSteps
    {
        private string _bidderLoginCookieValue;

        public BidSteps(ItemsContext itemsContext)
            : base(itemsContext)
        {
        }

        [When(@"I create the following bidders using the '(.*)' button")]
        public void WhenICreateTheFollowingBidders(string buttonName, Table table)
        {
            var biddersToCreate = table.CreateSet<BidderRegistrationViewModel>();
            var defaultTicket = GetDefaultTicketType().TicketTypeId;
            foreach (var bidder in biddersToCreate)
            {
                bidder.Students = new List<StudentViewModel>();

                bidder.AuctionGuests = new List<AuctionGuestViewModel>
                {
                    new AuctionGuestViewModel { FirstName = bidder.FirstName, LastName = bidder.LastName, TicketPrice = 1m, TicketType = defaultTicket.ToString() }
                };

                BiddersController.Register(bidder, buttonName, string.Empty);
            }
        }

        [Then(@"the bidders in the system are")]
        public void ThenTheBiddersInTheSystemAre(Table expected)
        {
            var actualBidders = (List<BidderForList>)(BiddersController.Index() as ViewResult).Model;
            expected.CompareToSet(actualBidders);
        }

        [When(@"I log in as bidder number '(.*)' with last name '(.*)' and email '(.*)'")]
        public void WhenILogInAsBidderNumberWithLastNameAndEmail(int bidderNumber, string lastName, string email)
        {
            var bidderLogin = new BidderLookupModel
            {
                BidderNumber = bidderNumber,
                LastName = lastName,
                Email = email
            };

            var actionResult = BiddersController.LoginConfirmed(bidderLogin);
            VerifyRedirectAction(actionResult, "Bids");

            _bidderLoginCookieValue = HttpContext.Current.Response.Cookies[BidderRepository.BidderIdCookieName].Value;
        }

        [Then(@"I enter a bid of '(.*)' for item number '(.*)'")]
        public void ThenIEnterABidOfForItemNumber(decimal bidAmount, int itemNumber)
        {
            //first log in as the bidder
            HttpContext.Current.Request.Cookies.Set(new System.Web.HttpCookie(BidderRepository.BidderIdCookieName, _bidderLoginCookieValue));

            var actionResult = BiddersController.EnterBidConfirmed(itemNumber, bidAmount);
            VerifyRedirectAction(actionResult, "Bids", "Bidders");
        }
    }
}
