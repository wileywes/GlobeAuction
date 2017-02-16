namespace GlobeAuction.Migrations
{
    using Models;
    using System;
    using System.Data.Entity.Migrations;

    public partial class InvoiceAndIpn : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        InvoiceId = c.Int(nullable: false, identity: true),
                        IsPaid = c.Boolean(nullable: false),
                        WasMarkedPaidManually = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                        Bidder_BidderId = c.Int(nullable: false),
                        PaymentTransaction_PayPalTransactionId = c.Int(),
                    })
                .PrimaryKey(t => t.InvoiceId)
                .ForeignKey("dbo.Bidders", t => t.Bidder_BidderId, cascadeDelete: true)
                .ForeignKey("dbo.PayPalTransactions", t => t.PaymentTransaction_PayPalTransactionId)
                .Index(t => t.Bidder_BidderId)
                .Index(t => t.PaymentTransaction_PayPalTransactionId);
            
            AddColumn("dbo.PayPalTransactions", "TransactionType", c => c.Int(nullable: false, defaultValue: (int)PayPalTransactionType.BidderCart));
            AddColumn("dbo.PayPalTransactions", "NotificationType", c => c.Int(nullable: false, defaultValue: (int)PayPalNotificationType.CartRedirect));
            AddColumn("dbo.PayPalTransactions", "IpnTrackId", c => c.String());
            AddColumn("dbo.PayPalTransactions", "Custom", c => c.String());
            AddColumn("dbo.AuctionItems", "Invoice_InvoiceId", c => c.Int());
            CreateIndex("dbo.AuctionItems", "Invoice_InvoiceId");
            AddForeignKey("dbo.AuctionItems", "Invoice_InvoiceId", "dbo.Invoices", "InvoiceId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Invoices", "PaymentTransaction_PayPalTransactionId", "dbo.PayPalTransactions");
            DropForeignKey("dbo.Invoices", "Bidder_BidderId", "dbo.Bidders");
            DropForeignKey("dbo.AuctionItems", "Invoice_InvoiceId", "dbo.Invoices");
            DropIndex("dbo.Invoices", new[] { "PaymentTransaction_PayPalTransactionId" });
            DropIndex("dbo.Invoices", new[] { "Bidder_BidderId" });
            DropIndex("dbo.AuctionItems", new[] { "Invoice_InvoiceId" });
            DropColumn("dbo.AuctionItems", "Invoice_InvoiceId");
            DropColumn("dbo.PayPalTransactions", "Custom");
            DropColumn("dbo.PayPalTransactions", "IpnTrackId");
            DropColumn("dbo.PayPalTransactions", "NotificationType");
            DropColumn("dbo.PayPalTransactions", "TransactionType");
            DropTable("dbo.Invoices");
        }
    }
}
