namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStoreItem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StoreItemPurchases",
                c => new
                    {
                        StoreItemPurchaseId = c.Int(nullable: false, identity: true),
                        Quantity = c.Int(nullable: false),
                        PricePaid = c.Decimal(precision: 18, scale: 2),
                        PurchaseTransaction_PayPalTransactionId = c.Int(),
                        StoreItem_StoreItemId = c.Int(nullable: false),
                        Bidder_BidderId = c.Int(),
                        Invoice_InvoiceId = c.Int(),
                    })
                .PrimaryKey(t => t.StoreItemPurchaseId)
                .ForeignKey("dbo.PayPalTransactions", t => t.PurchaseTransaction_PayPalTransactionId)
                .ForeignKey("dbo.StoreItems", t => t.StoreItem_StoreItemId, cascadeDelete: true)
                .ForeignKey("dbo.Bidders", t => t.Bidder_BidderId)
                .ForeignKey("dbo.Invoices", t => t.Invoice_InvoiceId)
                .Index(t => t.PurchaseTransaction_PayPalTransactionId)
                .Index(t => t.StoreItem_StoreItemId)
                .Index(t => t.Bidder_BidderId)
                .Index(t => t.Invoice_InvoiceId);
            
            CreateTable(
                "dbo.StoreItems",
                c => new
                    {
                        StoreItemId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 500, unicode: false),
                        Price = c.Int(nullable: false),
                        CanPurchaseInBidderRegistration = c.Boolean(nullable: false),
                        CanPurchaseInAuctionCheckout = c.Boolean(nullable: false),
                        CanPurchaseInStore = c.Boolean(nullable: false),
                        OnlyVisibleToAdmins = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                    })
                .PrimaryKey(t => t.StoreItemId)
                .Index(t => t.Title, unique: true, name: "IX_StoreItem_Title");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StoreItemPurchases", "Invoice_InvoiceId", "dbo.Invoices");
            DropForeignKey("dbo.StoreItemPurchases", "Bidder_BidderId", "dbo.Bidders");
            DropForeignKey("dbo.StoreItemPurchases", "StoreItem_StoreItemId", "dbo.StoreItems");
            DropForeignKey("dbo.StoreItemPurchases", "PurchaseTransaction_PayPalTransactionId", "dbo.PayPalTransactions");
            DropIndex("dbo.StoreItems", "IX_StoreItem_Title");
            DropIndex("dbo.StoreItemPurchases", new[] { "Invoice_InvoiceId" });
            DropIndex("dbo.StoreItemPurchases", new[] { "Bidder_BidderId" });
            DropIndex("dbo.StoreItemPurchases", new[] { "StoreItem_StoreItemId" });
            DropIndex("dbo.StoreItemPurchases", new[] { "PurchaseTransaction_PayPalTransactionId" });
            DropTable("dbo.StoreItems");
            DropTable("dbo.StoreItemPurchases");
        }
    }
}
