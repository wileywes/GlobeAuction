namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveDigitalEmailFlagToBid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bids", "HasWinnerBeenEmailed", c => c.Boolean(nullable: false, defaultValue:false));
            AddColumn("dbo.StoreItemPurchases", "HasWinnerBeenEmailed", c => c.Boolean(nullable: false, defaultValue: false));
            DropColumn("dbo.DonationItems", "HasWinnerBeenEmailed");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DonationItems", "HasWinnerBeenEmailed", c => c.Boolean(nullable: false));
            DropColumn("dbo.StoreItemPurchases", "HasWinnerBeenEmailed");
            DropColumn("dbo.Bids", "HasWinnerBeenEmailed");
        }
    }
}
