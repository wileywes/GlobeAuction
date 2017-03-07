namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvoiceForStoreItems : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "FirstName", c => c.String());
            AddColumn("dbo.Invoices", "LastName", c => c.String());
            AddColumn("dbo.Invoices", "Phone", c => c.String());
            AddColumn("dbo.Invoices", "Email", c => c.String());
            AddColumn("dbo.Invoices", "ZipCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "ZipCode");
            DropColumn("dbo.Invoices", "Email");
            DropColumn("dbo.Invoices", "Phone");
            DropColumn("dbo.Invoices", "LastName");
            DropColumn("dbo.Invoices", "FirstName");
        }
    }
}
