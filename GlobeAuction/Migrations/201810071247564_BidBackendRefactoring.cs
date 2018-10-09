namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidBackendRefactoring : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AuctionItems", "Invoice_InvoiceId", "dbo.Invoices");
            DropIndex("dbo.AuctionItems", new[] { "Invoice_InvoiceId" });
            CreateTable(
                "dbo.Bids",
                c => new
                    {
                        BidId = c.Int(nullable: false, identity: true),
                        BidAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AmountPaid = c.Decimal(precision: 18, scale: 2),
                        IsWinning = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                        AuctionItem_AuctionItemId = c.Int(nullable: false),
                        Bidder_BidderId = c.Int(nullable: false),
                        Invoice_InvoiceId = c.Int(),
                    })
                .PrimaryKey(t => t.BidId)
                .ForeignKey("dbo.AuctionItems", t => t.AuctionItem_AuctionItemId, cascadeDelete: true)
                .ForeignKey("dbo.Bidders", t => t.Bidder_BidderId, cascadeDelete: true)
                .ForeignKey("dbo.Invoices", t => t.Invoice_InvoiceId)
                .Index(t => t.AuctionItem_AuctionItemId)
                .Index(t => t.Bidder_BidderId)
                .Index(t => t.Invoice_InvoiceId);
            
            AddColumn("dbo.AuctionItems", "Quantity", c => c.Int(nullable: false));
            AddColumn("dbo.DonationItems", "Quantity", c => c.Int(nullable: false));
            AddColumn("dbo.Invoices", "TotalPaid", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.AuctionItems", "WinningBidderId");
            DropColumn("dbo.AuctionItems", "WinningBid");
            DropColumn("dbo.AuctionItems", "IsMasterItemForMultipleWinners");
            DropColumn("dbo.AuctionItems", "Invoice_InvoiceId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AuctionItems", "Invoice_InvoiceId", c => c.Int());
            AddColumn("dbo.AuctionItems", "IsMasterItemForMultipleWinners", c => c.Boolean(nullable: false));
            AddColumn("dbo.AuctionItems", "WinningBid", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.AuctionItems", "WinningBidderId", c => c.Int());
            DropForeignKey("dbo.Bids", "Invoice_InvoiceId", "dbo.Invoices");
            DropForeignKey("dbo.Bids", "Bidder_BidderId", "dbo.Bidders");
            DropForeignKey("dbo.Bids", "AuctionItem_AuctionItemId", "dbo.AuctionItems");
            DropIndex("dbo.Bids", new[] { "Invoice_InvoiceId" });
            DropIndex("dbo.Bids", new[] { "Bidder_BidderId" });
            DropIndex("dbo.Bids", new[] { "AuctionItem_AuctionItemId" });
            DropColumn("dbo.Invoices", "TotalPaid");
            DropColumn("dbo.DonationItems", "Quantity");
            DropColumn("dbo.AuctionItems", "Quantity");
            DropTable("dbo.Bids");
            CreateIndex("dbo.AuctionItems", "Invoice_InvoiceId");
            AddForeignKey("dbo.AuctionItems", "Invoice_InvoiceId", "dbo.Invoices", "InvoiceId");
        }
    }
}
