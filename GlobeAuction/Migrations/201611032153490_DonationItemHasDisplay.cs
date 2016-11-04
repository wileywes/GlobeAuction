namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DonationItemHasDisplay : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "HasDisplay", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "HasDisplay");
        }
    }
}
