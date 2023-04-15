using GlobeAuction.Helpers;
using GlobeAuction.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace GlobeAuction.Controllers
{
    public class EmailTemplatesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EmailTemplates
        public ActionResult Index()
        {
            return View(db.EmailTemplates.ToList());
        }

        // GET: EmailTemplates/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            if (emailTemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailTemplate);
        }

        // GET: EmailTemplates/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmailTemplates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Description,Subject,SupportedTokensCsvList,HtmlBody")] EmailTemplate emailTemplate)
        {
            if (ModelState.IsValid)
            {
                emailTemplate.CreateDate = emailTemplate.UpdateDate = Utilities.GetEasternTimeNow();
                emailTemplate.UpdateBy = User.Identity.GetUserName();

                db.EmailTemplates.Add(emailTemplate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(emailTemplate);
        }

        // GET: EmailTemplates/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            if (emailTemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailTemplate);
        }

        // POST: EmailTemplates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmailTemplateId,Name,Description,Subject,SupportedTokensCsvList,HtmlBody,CreateDate,UpdateDate,UpdateBy")] EmailTemplate emailTemplate)
        {
            if (ModelState.IsValid)
            {
                emailTemplate.UpdateDate = Utilities.GetEasternTimeNow();
                emailTemplate.UpdateBy = User.Identity.GetUserName();

                db.Entry(emailTemplate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(emailTemplate);
        }

        // GET: EmailTemplates/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            if (emailTemplate == null)
            {
                return HttpNotFound();
            }
            return View(emailTemplate);
        }

        // POST: EmailTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EmailTemplate emailTemplate = db.EmailTemplates.Find(id);
            db.EmailTemplates.Remove(emailTemplate);
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
