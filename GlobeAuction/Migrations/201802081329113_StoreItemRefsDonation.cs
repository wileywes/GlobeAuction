namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoreItemRefsDonation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StoreItems", "DonationItemCopiedFrom_DonationItemId", c => c.Int());
            CreateIndex("dbo.StoreItems", "DonationItemCopiedFrom_DonationItemId");
            AddForeignKey("dbo.StoreItems", "DonationItemCopiedFrom_DonationItemId", "dbo.DonationItems", "DonationItemId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StoreItems", "DonationItemCopiedFrom_DonationItemId", "dbo.DonationItems");
            DropIndex("dbo.StoreItems", new[] { "DonationItemCopiedFrom_DonationItemId" });
            DropColumn("dbo.StoreItems", "DonationItemCopiedFrom_DonationItemId");
        }
    }
}
