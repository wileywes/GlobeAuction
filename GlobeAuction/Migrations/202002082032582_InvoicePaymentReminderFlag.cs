namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvoicePaymentReminderFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "IsPaymentReminderSent", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[Invoices] SET IsPaymentReminderSent = 0");
            AlterColumn("dbo.Invoices", "IsPaymentReminderSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "IsPaymentReminderSent");
        }
    }
}
