namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DonationItemInStoreFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "IsInStore", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[DonationItems] SET IsInStore = 0");
            AlterColumn("dbo.DonationItems", "IsInStore", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "IsInStore");
        }
    }
}
