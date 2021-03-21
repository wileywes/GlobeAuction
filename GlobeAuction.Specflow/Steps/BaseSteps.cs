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
using System.Web;
using System.Security.Principal;
using NUnit.Framework;
using System.IO;

namespace GlobeAuction.Specflow
{
    [Binding]
    public abstract class BaseSteps
    {
        protected ApplicationDbContext _db = new ApplicationDbContext();

        protected ItemsContext ItemsContext;
        protected DonationItemsController DonationItemsController;
        protected AuctionCategoriesController AuctionCategoriesController;
        protected AuctionItemsController AuctionItemsController;
        protected BiddersController BiddersController;
        protected TicketTypesController TicketTypesController;

        public BaseSteps(ItemsContext itemsContext)
        {
            ItemsContext = itemsContext;
            DonationItemsController = SetupMocksOnController(new DonationItemsController());
            AuctionCategoriesController = SetupMocksOnController(new AuctionCategoriesController());
            AuctionItemsController = SetupMocksOnController(new AuctionItemsController());
            BiddersController = SetupMocksOnController(new BiddersController());
            TicketTypesController = SetupMocksOnController(new TicketTypesController());
        }

        private T SetupMocksOnController<T>(T controller) where T:Controller
        {
            var principal = new GenericPrincipal(new GenericIdentity("SpecflowUser"), null);
            var fakeHttpContext = new MockHttpContextBase { User = principal };
            var controllerContext = new ControllerContext
            {
                HttpContext = fakeHttpContext                
            };

            // Now set the controller ControllerContext with fake context
            controller.ControllerContext = controllerContext;
            return controller;
        }

        protected CatalogData GetCatalogData()
        {
            return new ItemsRepository(_db).GetCatalogData();
        }

        protected TicketType GetDefaultTicketType()
        {
            const string defaultTicketName = "SpecflowTicket";
            var existing = _db.TicketTypes.FirstOrDefault(t => t.Name == defaultTicketName);
            if (existing != null) return existing;

            var newTicket = new TicketType
            {
                Name = defaultTicketName,
                Price = 1m,
                UpdateBy = "Specflow"
            };
            TicketTypesController.Create(newTicket);
            return _db.TicketTypes.FirstOrDefault(t => t.Name == defaultTicketName);
        }

        protected void VerifyRedirectAction(ActionResult actionResult, string expectionAction)
        {
            Assert.That(actionResult, Is.InstanceOf(typeof(RedirectToRouteResult)));

            var routeResult = actionResult as RedirectToRouteResult;
            Assert.That(routeResult.RouteValues.ContainsKey("action"));
            Assert.That(routeResult.RouteValues["action"], Is.EqualTo(expectionAction));
        }

        protected void VerifyRedirectAction(ActionResult actionResult, string expectionAction, string expectedController)
        {
            Assert.That(actionResult, Is.InstanceOf(typeof(RedirectToRouteResult)));

            var routeResult = actionResult as RedirectToRouteResult;
            Assert.That(routeResult.RouteValues.ContainsKey("action"));
            Assert.That(routeResult.RouteValues["action"], Is.EqualTo(expectionAction));
            Assert.That(routeResult.RouteValues.ContainsKey("controller"));
            Assert.That(routeResult.RouteValues["controller"], Is.EqualTo(expectedController));
        }
    }

    public class MockHttpContextBase : HttpContextBase
    {
        public override IPrincipal User { get; set; }
        public override HttpRequestBase Request { get; }
        public override HttpResponseBase Response { get; }

        public MockHttpContextBase()
        {
            var request = new HttpRequest("", "http://www.test.com/", "");
            Request = new HttpRequestWrapper(request);

            var response = new HttpResponse(new StringWriter());
            Response = new HttpResponseWrapper(response);

            HttpContext.Current = new HttpContext(request, response);
        }
    }
}
