namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItemNumberRangeForAuctionCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionCategories", "ItemNumberStart", c => c.Int());
            AddColumn("dbo.AuctionCategories", "ItemNumberEnd", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionCategories", "ItemNumberEnd");
            DropColumn("dbo.AuctionCategories", "ItemNumberStart");
        }
    }
}
