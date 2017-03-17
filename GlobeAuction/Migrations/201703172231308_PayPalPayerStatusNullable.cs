namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PayPalPayerStatusNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PayPalTransactions", "PayerStatus", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PayPalTransactions", "PayerStatus", c => c.String(nullable: false));
        }
    }
}
