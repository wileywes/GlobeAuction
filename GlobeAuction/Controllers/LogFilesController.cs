using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Models;
using Microsoft.AspNet.Identity;
using System.Configuration;
using GlobeAuction.Helpers;
using System.IO;

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanAdminUsers)]
    public class LogFilesController : Controller
    {
        // GET: Logs
        public ActionResult Index()
        {
            var dir = Server.MapPath("/logs");
            var logs = new List<LogFile>();

            if (Directory.Exists(dir))
            {
                logs = Directory.GetFiles(dir).Select(f =>
                new LogFile
                {
                    FilePath = f,
                    Content = System.IO.File.ReadAllText(f)
                }).ToList();

                return View(logs);
            }

            return View(logs);
        }
    }
}
