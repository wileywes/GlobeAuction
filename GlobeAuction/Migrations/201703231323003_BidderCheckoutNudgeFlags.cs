namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderCheckoutNudgeFlags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bidders", "IsCheckoutNudgeEmailSent", c => c.Boolean(nullable: false, defaultValue:false));
            AddColumn("dbo.Bidders", "IsCheckoutNudgeTextSent", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bidders", "IsCheckoutNudgeTextSent");
            DropColumn("dbo.Bidders", "IsCheckoutNudgeEmailSent");
        }
    }
}
