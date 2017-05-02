using System.Data.Entity;
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

            base.OnModelCreating(modelBuilder);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<GlobeAuction.Models.DonationItem> DonationItems { get; set; }
        public System.Data.Entity.DbSet<GlobeAuction.Models.Solicitor> Solicitors { get; set; }
        public System.Data.Entity.DbSet<GlobeAuction.Models.AuctionItem> AuctionItems { get; set; }
        public System.Data.Entity.DbSet<GlobeAuction.Models.Bidder> Bidders { get; set; }
        public System.Data.Entity.DbSet<GlobeAuction.Models.TicketType> TicketTypes { get; set; }

        public System.Data.Entity.DbSet<GlobeAuction.Models.AuctionGuest> AuctionGuests { get; set; }
        public System.Data.Entity.DbSet<GlobeAuction.Models.Student> Students { get; set; }
        public DbSet<PayPalTransaction> PayPalTransactions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<StoreItem> StoreItems { get; set; }
        public DbSet<StoreItemPurchase> StoreItemPurchases { get; set; }

        public System.Data.Entity.DbSet<GlobeAuction.Models.Donor> Donors { get; set; }
    }
}