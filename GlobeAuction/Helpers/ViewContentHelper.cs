using GlobeAuction.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GlobeAuction.Helpers
{
    public static class ViewContentHelper
    {
        private static List<string> _carouselFileNames;

        public static MenuLayout GetMenuLayout()
        {
            var allEnabledButtons = new List<MenuLayoutButton>();

            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowRegisterIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/register",
                    ImageUrl = "~/Content/Images/icon_register.png",
                    Name = "REGISTER"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowDonateIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/donate",
                    ImageUrl = "~/Content/Images/icon_Donate.png",
                    Name = "DONATE"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowCatalogIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/catalog",
                    ImageUrl = "~/Content/Images/icon_catalog.png",
                    Name = "CATALOG"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowBidIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/bidders/bids",
                    ImageUrl = "~/Content/Images/icon_bid.png",
                    Name = "BIDS"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowFaqIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/faqs",
                    ImageUrl = "~/Content/Images/icon_faqs.png",
                    Name = "FAQs"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowStoreIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/store",
                    ImageUrl = "~/Content/Images/icon_store.png",
                    Name = "STORE"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowSponsorsIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/home/sponsors",
                    ImageUrl = "~/Content/Images/icon_sponsors.png",
                    Name = "SPONSORS"
                });
            }
            if (ConfigHelper.GetConfigValue<bool>(ConfigNames.HomePage_ShowShoutOutsIcon, false))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = "/home/shoutouts",
                    ImageUrl = "~/Content/Images/icon_shoutout.png",
                    Name = "SHOUT OUTS"
                });
            }

            var volunteerLink = ConfigHelper.GetConfigValue(ConfigNames.HomePage_VolunteerLink);
            if (!string.IsNullOrEmpty(volunteerLink))
            {
                allEnabledButtons.Add(new MenuLayoutButton
                {
                    Href = volunteerLink,
                    ImageUrl = "~/Content/Images/icon_Volunteers.png",
                    Name = "VOLUNTEERS",
                    Id = "loginLink"
                });
            }

            //add in the Home button if we have an odd number right now
            if (allEnabledButtons.Count % 2 == 1)
            {
                allEnabledButtons.Insert(0, new MenuLayoutButton
                {
                    Href = "/Home",
                    ImageUrl = "~/Content/Images/icon_Home.png",
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

        public static List<string> GetCarouselFileNames()
        {
            if (_carouselFileNames == null)
            {
                _carouselFileNames = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/Content/Images/Carousel/"))
                    .Select(path => new FileInfo(path).Name)
                    .ToList();
            }

            return _carouselFileNames;
        }
    }
}