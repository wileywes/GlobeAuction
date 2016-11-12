namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBidders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bidders",
                c => new
                    {
                        BidderId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        ZipCode = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BidderId);
            
            CreateTable(
                "dbo.AuctionGuests",
                c => new
                    {
                        AuctionGuestId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        TicketType = c.String(),
                        TicketPricePaid = c.Int(),
                        Bidder_BidderId = c.Int(),
                    })
                .PrimaryKey(t => t.AuctionGuestId)
                .ForeignKey("dbo.Bidders", t => t.Bidder_BidderId)
                .Index(t => t.Bidder_BidderId);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        StudentId = c.Int(nullable: false, identity: true),
                        HomeroomTeacher = c.String(),
                        Bidder_BidderId = c.Int(),
                    })
                .PrimaryKey(t => t.StudentId)
                .ForeignKey("dbo.Bidders", t => t.Bidder_BidderId)
                .Index(t => t.Bidder_BidderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Students", "Bidder_BidderId", "dbo.Bidders");
            DropForeignKey("dbo.AuctionGuests", "Bidder_BidderId", "dbo.Bidders");
            DropIndex("dbo.Students", new[] { "Bidder_BidderId" });
            DropIndex("dbo.AuctionGuests", new[] { "Bidder_BidderId" });
            DropTable("dbo.Students");
            DropTable("dbo.AuctionGuests");
            DropTable("dbo.Bidders");
        }
    }
}
