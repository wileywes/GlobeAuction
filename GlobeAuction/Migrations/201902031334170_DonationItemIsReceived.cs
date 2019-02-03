namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DonationItemIsReceived : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "IsReceived", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[DonationItems] SET IsReceived = 0");
            AlterColumn("dbo.DonationItems", "IsReceived", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "IsReceived");
        }
    }
}
