using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobeAuction.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net;

namespace GlobeAuction.Controllers
{
    [Authorize(Roles = AuctionRoles.CanAdminUsers)]
    public class UsersAdminController : Controller
    {
        public UsersAdminController()
        {
            db = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }

        public UsersAdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public ApplicationDbContext db { get; private set; }
        
        public ActionResult AdminFunctions()
        {
            return View();
        }

        public ActionResult Index()
        {
            //must to ToList() to close out the DataReader from Users
            return View(UserManager.Users.ToList()
                .Select(u => new ApplicationUserWithRoleNames(u, RoleManager))
                .ToList());
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            AddUserEditControlInfo(user);
            return View(user);
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UserName,Id,HomeTown")] ApplicationUser formuser, string id, string[] roleIds)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            AddUserEditControlInfo(user);
            user.UserName = formuser.UserName;
            if (ModelState.IsValid)
            {
                //Update the user details
                await UserManager.UpdateAsync(user);

                //If user has existing Role then remove the user from the role
                // This also accounts for the case when the Admin selected Empty from the drop-down and
                // this means that all roles for the user must be removed
                var rolesForUser = await UserManager.GetRolesAsync(id);
                if (rolesForUser.Count() > 0)
                {
                    foreach (var item in rolesForUser)
                    {
                        var result = await UserManager.RemoveFromRoleAsync(id, item);
                    }
                }
                
                if (roleIds != null && roleIds.Any())
                {
                    foreach (var roleId in roleIds)
                    {
                        //Find Role
                        var role = await RoleManager.FindByIdAsync(roleId);
                        //Add user to new role
                        var result = await UserManager.AddToRoleAsync(id, role.Name);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First().ToString());
                            return View();
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }


        private void AddUserEditControlInfo(ApplicationUser user)
        {
            var myRoles = user == null ? new List<string>() : user.Roles.Select(r => r.RoleId).ToList();
            var availableRoles = RoleManager.Roles.Select(r => new
            {
                RoleId = r.Id,
                RoleName = r.Name
            }).ToList();

            ViewBag.AvailableRoles = new MultiSelectList(availableRoles, "RoleId", "RoleName", myRoles);
        }

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var viewModel = new ApplicationUserWithRoleNames(user, RoleManager);
            return View(viewModel);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                db.Users.Remove(user);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }
    }
}