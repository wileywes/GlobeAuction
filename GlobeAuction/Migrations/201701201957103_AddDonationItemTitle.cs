namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDonationItemTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "Title", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "Title");
        }
    }
}
