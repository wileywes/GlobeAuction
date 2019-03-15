namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderCatalogFavorites : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CatalogFavorites",
                c => new
                    {
                        CatalogFavoriteId = c.Int(nullable: false, identity: true),
                        AuctionItem_AuctionItemId = c.Int(),
                        Bidder_BidderId = c.Int(),
                    })
                .PrimaryKey(t => t.CatalogFavoriteId)
                .ForeignKey("dbo.AuctionItems", t => t.AuctionItem_AuctionItemId, cascadeDelete: true)
                .ForeignKey("dbo.Bidders", t => t.Bidder_BidderId, cascadeDelete: true)
                .Index(t => t.AuctionItem_AuctionItemId)
                .Index(t => t.Bidder_BidderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CatalogFavorites", "Bidder_BidderId", "dbo.Bidders");
            DropForeignKey("dbo.CatalogFavorites", "AuctionItem_AuctionItemId", "dbo.AuctionItems");
            DropIndex("dbo.CatalogFavorites", new[] { "Bidder_BidderId" });
            DropIndex("dbo.CatalogFavorites", new[] { "AuctionItem_AuctionItemId" });
            DropTable("dbo.CatalogFavorites");
        }
    }
}
