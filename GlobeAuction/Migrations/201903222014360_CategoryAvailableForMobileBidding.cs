namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CategoryAvailableForMobileBidding : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionCategories", "IsAvailableForMobileBidding", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[AuctionCategories] SET IsAvailableForMobileBidding = 1");
            AlterColumn("dbo.AuctionCategories", "IsAvailableForMobileBidding", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionCategories", "IsAvailableForMobileBidding");
        }
    }
}
