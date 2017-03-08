namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WinningBidDecimal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AuctionItems", "WinningBid", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AuctionItems", "WinningBid", c => c.Int());
        }
    }
}
