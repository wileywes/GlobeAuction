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
    public class CarouselPicturesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly string imageFolderPath = "~/Content/images/Carousel/";

        // GET: CarouselPictures
        public ActionResult Index()
        {
            string serverPath = Server.MapPath(imageFolderPath);
            if (!Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }

            List<string> images = new List<string>();
            foreach (string filePath in Directory.GetFiles(serverPath))
            {
                images.Add("/Content/images/Carousel/" + Path.GetFileName(filePath));
            }

            return View(images);
        }

        // POST: Upload Image
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string fileName = Path.GetFileName(file.FileName);
                string fullPath = Path.Combine(Server.MapPath(imageFolderPath), fileName);

                file.SaveAs(fullPath);
            }
            return RedirectToAction("Index");
        }

        // POST: Delete Image
        [HttpPost]
        public ActionResult Delete(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string fullPath = Server.MapPath(imageFolderPath + fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
