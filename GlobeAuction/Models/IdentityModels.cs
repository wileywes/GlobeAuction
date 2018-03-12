using System;
using System.Data.Entity;
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

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            //Database.SetInitializer<ApplicationDbContext>(new DropCreateDatabaseIfModelChanges<ApplicationDbContext>());
            //Database.SetInitializer<ApplicationDbContext>(new DropCreateDatabaseAlways<ApplicationDbContext>());
            Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //fluent API hook-in here
            modelBuilder.Entity<StoreItemPurchase>()
                        .HasRequired(t => t.StoreItem)
                        .WithMany(t => t.StoreItemPurchases)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<StoreItem>()
                .HasMany(g => g.BundleComponents)
                .WithRequired()
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<DonationItem> DonationItems { get; set; }
        public DbSet<Solicitor> Solicitors { get; set; }
        public DbSet<AuctionItem> AuctionItems { get; set; }
        public DbSet<Bidder> Bidders { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }

        public DbSet<AuctionGuest> AuctionGuests { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<PayPalTransaction> PayPalTransactions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<StoreItem> StoreItems { get; set; }
        public DbSet<BundleComponent> BundleComponents { get; set; }
        public DbSet<StoreItemPurchase> StoreItemPurchases { get; set; }

        public DbSet<Donor> Donors { get; set; }
    }
}