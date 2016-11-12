namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GuestTicketPricePaidIsDecimal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AuctionGuests", "TicketPricePaid", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AuctionGuests", "TicketPricePaid", c => c.Int());
        }
    }
}
