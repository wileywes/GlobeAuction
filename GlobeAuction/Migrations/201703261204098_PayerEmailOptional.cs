namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PayerEmailOptional : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PayPalTransactions", "PayerEmail", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PayPalTransactions", "PayerEmail", c => c.String(nullable: false));
        }
    }
}
