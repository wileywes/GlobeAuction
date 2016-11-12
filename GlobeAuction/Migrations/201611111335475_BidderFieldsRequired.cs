namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderFieldsRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Bidders", "FirstName", c => c.String(nullable: false));
            AlterColumn("dbo.Bidders", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.Bidders", "Phone", c => c.String(nullable: false));
            AlterColumn("dbo.Bidders", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.AuctionGuests", "FirstName", c => c.String(nullable: false));
            AlterColumn("dbo.AuctionGuests", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.AuctionGuests", "TicketType", c => c.String(nullable: false));
            AlterColumn("dbo.TicketTypes", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TicketTypes", "Name", c => c.String());
            AlterColumn("dbo.AuctionGuests", "TicketType", c => c.String());
            AlterColumn("dbo.AuctionGuests", "LastName", c => c.String());
            AlterColumn("dbo.AuctionGuests", "FirstName", c => c.String());
            AlterColumn("dbo.Bidders", "Email", c => c.String());
            AlterColumn("dbo.Bidders", "Phone", c => c.String());
            AlterColumn("dbo.Bidders", "LastName", c => c.String());
            AlterColumn("dbo.Bidders", "FirstName", c => c.String());
        }
    }
}
