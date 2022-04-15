﻿using GlobeAuction.Models;
using System.Collections.Generic;
using System.Linq;

namespace GlobeAuction.Helpers
{
    public static class ViewContentHelper
    {
        public static MenuLayout GetMenuLayout()
        {
            var allEnabledButtons = new List<MenuLayoutButton>();

            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowRegisterIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/register",
                    ImageUrl = "~/Content/Images/h_Register2.gif",
                    Name = "REGISTER"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowDonateIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/donate",
                    ImageUrl = "~/Content/Images/h_Donate.gif",
                    Name = "DONATE"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowCatalogIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/catalog",
                    ImageUrl = "~/Content/Images/h_catalog.gif",
                    Name = "CATALOG"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowBidIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/bidders/bids",
                    ImageUrl = "~/Content/Images/h_bid.gif",
                    Name = "BIDS"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowFaqIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/faqs",
                    ImageUrl = "~/Content/Images/h_faq.gif",
                    Name = "FAQs"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowStoreIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/store",
                    ImageUrl = "~/Content/Images/h_store.gif",
                    Name = "STORE"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowSponsorsIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/home/sponsors",
                    ImageUrl = "~/Content/Images/h_sponsors.gif",
                    Name = "SPONSORS"
                });
            }

            allEnabledButtons.Add(new MenuLayoutButton
            {
                Href = "/account/login",
                ImageUrl = "~/Content/Images/h_Volunteer.gif",
                Name = "VOLUNTEERS",
                Id = "loginLink"
            }); ;

            //add in the Home button if we have an odd number right now
            if (allEnabledButtons.Count % 2 == 1)
            {
                allEnabledButtons.Insert(0, new MenuLayoutButton
                {
                    Href = "/Home",
                    ImageUrl = "~/Content/Images/h_Home.gif",
                    Name = "HOME"
                });
            }

            var threshold = allEnabledButtons.Count / 2;

            return new MenuLayout
            {
                LeftButtons = allEnabledButtons.Take(threshold).ToList(),
                RightButtons = allEnabledButtons.Skip(threshold).ToList()
            };
        }
    }
}