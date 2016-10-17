
namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;

    internal sealed class Configuration : DbMigrationsConfiguration<GlobeAuction.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "GlobeAuction.Models.ApplicationDbContext";
        }

        protected override void Seed(GlobeAuction.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            SetupRoles(context);
        }

        private void SetupRoles(ApplicationDbContext context)
        {
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var ir = rm.Create(new IdentityRole(Roles.CanCheckoutWinners));
            ir = rm.Create(new IdentityRole(Roles.CanEditItems));
            ir = rm.Create(new IdentityRole(Roles.CanEditWinners));

            /*
            var um = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));
            var user = new ApplicationUser()
            {
                UserName = "user1@contoso.com",
            };
            ir = um.Create(user, "P_assw0rd1");
            if (ir.Succeeded == false)
                return ir.Succeeded;
            ir = um.AddToRole(user.Id, "canEdit");
            return ir.Succeeded;*/
        }
    }
}
