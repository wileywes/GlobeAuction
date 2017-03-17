﻿using System.Web.Mvc;
using System.Web.Routing;

namespace GlobeAuction
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Donate",
                url: "donate",
                defaults: new { controller = "DonationItems", action = "Create" }
            );

            routes.MapRoute(
                name: "Register",
                url: "register",
                defaults: new { controller = "Bidders", action = "Register" }
            );

            routes.MapRoute(
                name: "Shop",
                url: "shop",
                defaults: new { controller = "StoreItems", action = "Buy" }
                );

            routes.MapRoute(
                name: "Pay",
                url: "pay",
                defaults: new { controller = "Invoices", action = "Checkout" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
