namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsDonationItemGiftCard : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "IsGiftCard", c => c.Boolean(nullable: false, defaultValue:false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "IsGiftCard");
        }
    }
}
