namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPayPalTransaction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PayPalTransactions",
                c => new
                    {
                        PayPalTransactionId = c.Int(nullable: false, identity: true),
                        PaymentDate = c.DateTime(nullable: false),
                        PayerId = c.String(nullable: false),
                        PaymentGross = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentType = c.String(nullable: false),
                        PayerEmail = c.String(nullable: false),
                        PayerStatus = c.String(nullable: false),
                        TxnId = c.String(nullable: false),
                        PaymentStatus = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.PayPalTransactionId);
            
            AddColumn("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId", c => c.Int());
            CreateIndex("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId");
            AddForeignKey("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId", "dbo.PayPalTransactions", "PayPalTransactionId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId", "dbo.PayPalTransactions");
            DropIndex("dbo.AuctionGuests", new[] { "TicketTransaction_PayPalTransactionId" });
            DropColumn("dbo.AuctionGuests", "TicketTransaction_PayPalTransactionId");
            DropTable("dbo.PayPalTransactions");
        }
    }
}
