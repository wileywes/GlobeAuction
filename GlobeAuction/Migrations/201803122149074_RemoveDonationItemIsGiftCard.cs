namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDonationItemIsGiftCard : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DonationItems", "IsGiftCard");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DonationItems", "IsGiftCard", c => c.Boolean(nullable: false));
        }
    }
}
