using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Helpers;
using GlobeAuction.Models;

namespace GlobeAuction.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Sponsors()
        {
            var levels = ConfigHelper.GetLineSeparatedConfig(ConfigNames.SponsorLevelsOrdered);
            var allSponsors = db.Sponsors.ToList();
            var viewModel = new SponsorsViewModel
            {
                SponsorsByLevel = new Dictionary<string, List<Sponsor>>()
            };

            foreach(var level in levels)
            {
                var sponsorsAtLevel = allSponsors.Where(s => s.Level == level).ToList();
                if (sponsorsAtLevel.Any())
                {
                    viewModel.SponsorsByLevel.Add(level, sponsorsAtLevel);
                }
            }
            
            return View(viewModel);
        }

        [AllowAnonymous]
        public ActionResult Faqs()
        {
            var model = new FaqsViewModel
            {
                CategoriesWithFAQs = new List<FaqCategoryViewModel>()
            };

            foreach (var cat in db.FaqCategories.ToList())
            {
                var faqsInCat = db.Faqs.Where(f => f.Category.FaqCategoryId == cat.FaqCategoryId).ToList();
                model.CategoriesWithFAQs.Add(new FaqCategoryViewModel
                {
                    Category = cat,
                    FAQs = faqsInCat
                });
            }
            return View(model);
        }
    }
}