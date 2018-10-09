using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GlobeAuction.Models
{
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
        public DbSet<Bid> Bids { get; set; }
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