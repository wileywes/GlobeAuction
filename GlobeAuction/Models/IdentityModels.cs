﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace GlobeAuction.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreateDate { get; set; }
        public DateTime? LastLogin { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationUserWithRoleNames
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public List<string> RoleNames { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastLogin { get; set; }

        public ApplicationUserWithRoleNames(ApplicationUser user, RoleManager<IdentityRole> roleManager)
        {
            Id = user.Id;
            UserName = user.UserName;
            RoleNames = user.Roles.Select(r => roleManager.FindById(r.RoleId).Name).ToList();
            CreateDate = user.CreateDate;
            LastLogin = user.LastLogin;
        }
    }
}