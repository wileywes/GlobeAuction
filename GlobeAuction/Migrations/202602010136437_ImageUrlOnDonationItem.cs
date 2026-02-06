namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImageUrlOnDonationItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "ImageUrl", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "ImageUrl");
        }
    }
}
