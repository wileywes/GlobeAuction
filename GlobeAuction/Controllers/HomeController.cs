using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
            return View();
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