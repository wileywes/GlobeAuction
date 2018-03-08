namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PaymentMethodOonInvoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "PaymentMethod", c => c.Int());
            Sql("UPDATE [dbo].[Invoices] SET PaymentMethod = 0 WHERE IsPaid=1 and WasMarkedPaidManually=0"); //PayPal
            Sql("UPDATE [dbo].[Invoices] SET PaymentMethod = 1 WHERE IsPaid=1 and WasMarkedPaidManually=1"); //Cash
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "PaymentMethod");
        }
    }
}
