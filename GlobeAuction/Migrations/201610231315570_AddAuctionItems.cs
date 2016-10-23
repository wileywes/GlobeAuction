namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuctionItems : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuctionItems",
                c => new
                    {
                        AuctionItemId = c.Int(nullable: false, identity: true),
                        UniqueItemNumber = c.Int(nullable: false),
                        Description = c.String(nullable: false),
                        Category = c.String(nullable: false),
                        StartingBid = c.Int(nullable: false),
                        BidIncrement = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                        WinningBidderId = c.Int(nullable: false),
                        WinningBid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AuctionItemId);
            
            AddColumn("dbo.DonationItems", "AuctionItem_AuctionItemId", c => c.Int());
            CreateIndex("dbo.DonationItems", "AuctionItem_AuctionItemId");
            AddForeignKey("dbo.DonationItems", "AuctionItem_AuctionItemId", "dbo.AuctionItems", "AuctionItemId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DonationItems", "AuctionItem_AuctionItemId", "dbo.AuctionItems");
            DropIndex("dbo.DonationItems", new[] { "AuctionItem_AuctionItemId" });
            DropColumn("dbo.DonationItems", "AuctionItem_AuctionItemId");
            DropTable("dbo.AuctionItems");
        }
    }
}
