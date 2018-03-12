namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderPaidManually : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bidders", "WasMarkedPaidManually", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[Bidders] SET WasMarkedPaidManually = 0");
            AlterColumn("dbo.Bidders", "WasMarkedPaidManually", c => c.Boolean(nullable: false));

            AddColumn("dbo.Bidders", "PaymentMethod", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bidders", "PaymentMethod");
            DropColumn("dbo.Bidders", "WasMarkedPaidManually");
        }
    }
}
