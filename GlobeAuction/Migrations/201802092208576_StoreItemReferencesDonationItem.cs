namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoreItemReferencesDonationItem : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.StoreItems", "IX_StoreItem_Title");
            AddColumn("dbo.StoreItems", "DonationItem_DonationItemId", c => c.Int());
            CreateIndex("dbo.StoreItems", "DonationItem_DonationItemId");
            AddForeignKey("dbo.StoreItems", "DonationItem_DonationItemId", "dbo.DonationItems", "DonationItemId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StoreItems", "DonationItem_DonationItemId", "dbo.DonationItems");
            DropIndex("dbo.StoreItems", new[] { "DonationItem_DonationItemId" });
            DropColumn("dbo.StoreItems", "DonationItem_DonationItemId");
            CreateIndex("dbo.StoreItems", "Title", unique: true, name: "IX_StoreItem_Title");
        }
    }
}
