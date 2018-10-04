namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CascadeDeleteTicketPurchasesWhenInvoiceDeleted : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TicketPurchases", "Invoice_InvoiceId", "dbo.Invoices");
            DropIndex("dbo.TicketPurchases", new[] { "Invoice_InvoiceId" });
            AlterColumn("dbo.Invoices", "UpdateBy", c => c.String(nullable: false));
            AlterColumn("dbo.TicketPurchases", "Invoice_InvoiceId", c => c.Int(nullable: false));
            CreateIndex("dbo.TicketPurchases", "Invoice_InvoiceId");
            AddForeignKey("dbo.TicketPurchases", "Invoice_InvoiceId", "dbo.Invoices", "InvoiceId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TicketPurchases", "Invoice_InvoiceId", "dbo.Invoices");
            DropIndex("dbo.TicketPurchases", new[] { "Invoice_InvoiceId" });
            AlterColumn("dbo.TicketPurchases", "Invoice_InvoiceId", c => c.Int());
            AlterColumn("dbo.Invoices", "UpdateBy", c => c.String());
            CreateIndex("dbo.TicketPurchases", "Invoice_InvoiceId");
            AddForeignKey("dbo.TicketPurchases", "Invoice_InvoiceId", "dbo.Invoices", "InvoiceId");
        }
    }
}
