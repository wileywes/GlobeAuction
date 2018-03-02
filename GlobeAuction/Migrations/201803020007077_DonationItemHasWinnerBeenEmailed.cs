namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DonationItemHasWinnerBeenEmailed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "HasWinnerBeenEmailed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "HasWinnerBeenEmailed");
        }
    }
}
