namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderCatalogNudgeFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bidders", "IsCatalogNudgeEmailSent", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[Bidders] SET IsCatalogNudgeEmailSent = 0");
            AlterColumn("dbo.Bidders", "IsCatalogNudgeEmailSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bidders", "IsCatalogNudgeEmailSent");
        }
    }
}
