namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderRequiredUpdates : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AuctionGuests", "FirstName", c => c.String());
            AlterColumn("dbo.AuctionGuests", "LastName", c => c.String());
            AlterColumn("dbo.AuctionGuests", "TicketType", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AuctionGuests", "TicketType", c => c.String(nullable: false));
            AlterColumn("dbo.AuctionGuests", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.AuctionGuests", "FirstName", c => c.String(nullable: false));
        }
    }
}
