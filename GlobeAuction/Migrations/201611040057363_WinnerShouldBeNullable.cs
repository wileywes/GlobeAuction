namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WinnerShouldBeNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AuctionItems", "WinningBidderId", c => c.Int());
            AlterColumn("dbo.AuctionItems", "WinningBid", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AuctionItems", "WinningBid", c => c.Int(nullable: false));
            AlterColumn("dbo.AuctionItems", "WinningBidderId", c => c.Int(nullable: false));
        }
    }
}
