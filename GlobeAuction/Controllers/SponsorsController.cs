using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Helpers;
using GlobeAuction.Models;
using Microsoft.AspNet.Identity;

namespace GlobeAuction
{
    public class SponsorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Sponsors
        public ActionResult Index()
        {
            return View(db.Sponsors.ToList());
        }
        
        // GET: Sponsors/Create
        public ActionResult Create()
        {
            AddControlInfo(null);
            return View();
        }

        // POST: Sponsors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string level, IEnumerable<HttpPostedFileBase> files)
        {
            var sponsor = new Sponsor
            {
                Level = level
            };

            if (ModelState.IsValid)
            {
                var file = files.FirstOrDefault(f => f != null && f.ContentLength > 0);
                if (file == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                const string pathBase = "~/Content/images/sponsors";
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath(pathBase), fileName);
                file.SaveAs(path);

                sponsor.ImageUrl = pathBase + "/" + fileName;

                sponsor.CreateDate = sponsor.UpdateDate = Utilities.GetEasternTimeNow();
                sponsor.UpdateBy = User.Identity.GetUserName();

                db.Sponsors.Add(sponsor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            AddControlInfo(level);

            return View(sponsor);
        }


        // GET: Sponsors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sponsor sponsor = db.Sponsors.Find(id);
            if (sponsor == null)
            {
                return HttpNotFound();
            }
            return View(sponsor);
        }

        // POST: Sponsors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sponsor sponsor = db.Sponsors.Find(id);
            db.Sponsors.Remove(sponsor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddControlInfo(string selectedLevel)
        {
            var levels = ConfigHelper.GetLineSeparatedConfig(ConfigNames.SponsorLevelsOrdered);
            var levelSelect = levels.Select(c => new SelectListItem { Text = c, Value = c }).ToList();

            if (!string.IsNullOrEmpty(selectedLevel))
            {
                var selected = levelSelect.FirstOrDefault(c => c.Value.Equals(selectedLevel));
                if (selected != null) selected.Selected = true;
            }

            ViewBag.SponsorLevels = levelSelect;
        }
    }
}
