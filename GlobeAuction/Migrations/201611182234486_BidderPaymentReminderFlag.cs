namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderPaymentReminderFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bidders", "IsPaymentReminderSent", c => c.Boolean(nullable: false));
            DropColumn("dbo.Bidders", "IsDeleted");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Bidders", "IsDeleted", c => c.Boolean(nullable: false));
            DropColumn("dbo.Bidders", "IsPaymentReminderSent");
        }
    }
}
