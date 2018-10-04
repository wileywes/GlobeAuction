namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvoiceForBidders : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId", "dbo.PayPalTransactions");
            DropIndex("dbo.AuctionGuests", new[] { "TicketTransaction_PayPalTransactionId" });
            CreateTable(
                "dbo.TicketPurchases",
                c => new
                    {
                        TicketPurchaseId = c.Int(nullable: false, identity: true),
                        TicketType = c.String(),
                        TicketPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TicketPricePaid = c.Decimal(precision: 18, scale: 2),
                        AuctionGuest_AuctionGuestId = c.Int(),
                        Invoice_InvoiceId = c.Int(),
                    })
                .PrimaryKey(t => t.TicketPurchaseId)
                .ForeignKey("dbo.AuctionGuests", t => t.AuctionGuest_AuctionGuestId)
                .ForeignKey("dbo.Invoices", t => t.Invoice_InvoiceId)
                .Index(t => t.AuctionGuest_AuctionGuestId)
                .Index(t => t.Invoice_InvoiceId);
            
            AddColumn("dbo.Invoices", "InvoiceType", c => c.Int(nullable: false));
            DropColumn("dbo.AuctionGuests", "TicketPricePaid");
            DropColumn("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId");
            DropColumn("dbo.Bidders", "WasMarkedPaidManually");
            DropColumn("dbo.Bidders", "PaymentMethod");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Bidders", "PaymentMethod", c => c.Int());
            AddColumn("dbo.Bidders", "WasMarkedPaidManually", c => c.Boolean(nullable: false));
            AddColumn("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId", c => c.Int());
            AddColumn("dbo.AuctionGuests", "TicketPricePaid", c => c.Decimal(precision: 18, scale: 2));
            DropForeignKey("dbo.TicketPurchases", "Invoice_InvoiceId", "dbo.Invoices");
            DropForeignKey("dbo.TicketPurchases", "AuctionGuest_AuctionGuestId", "dbo.AuctionGuests");
            DropIndex("dbo.TicketPurchases", new[] { "Invoice_InvoiceId" });
            DropIndex("dbo.TicketPurchases", new[] { "AuctionGuest_AuctionGuestId" });
            DropColumn("dbo.Invoices", "InvoiceType");
            DropTable("dbo.TicketPurchases");
            CreateIndex("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId");
            AddForeignKey("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId", "dbo.PayPalTransactions", "PayPalTransactionId");
        }
    }
}
