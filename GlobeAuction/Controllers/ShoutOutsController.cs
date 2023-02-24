using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
    public class ShoutOutsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ShoutOuts
        public ActionResult Index()
        {
            return View(db.ShoutOuts.ToList());
        }
        
        // GET: ShoutOuts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sponsors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string title, string bodyText, IEnumerable<HttpPostedFileBase> files)
        {
            var shoutOut = new ShoutOut
            {
                Title = title,
                BodyText = bodyText
            };

            if (ModelState.IsValid)
            {
                var file = files.FirstOrDefault(f => f != null && f.ContentLength > 0);
                if (file == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                const string pathBase = "~/Content/images/shoutouts";
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath(pathBase), fileName);
                file.SaveAs(path);

                shoutOut.ImageUrl = pathBase + "/" + fileName;

                shoutOut.CreateDate = shoutOut.UpdateDate = Utilities.GetEasternTimeNow();
                shoutOut.UpdateBy = User.Identity.GetUserName();

                db.ShoutOuts.Add(shoutOut);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(shoutOut);
        }


        // GET: ShoutOuts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShoutOut shoutOut = db.ShoutOuts.Find(id);
            if (shoutOut == null)
            {
                return HttpNotFound();
            }
            return View(shoutOut);
        }

        // POST: ShoutOuts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ShoutOut shoutOut = db.ShoutOuts.Find(id);
            db.ShoutOuts.Remove(shoutOut);
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
    }
}
