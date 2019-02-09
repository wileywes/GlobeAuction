namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuctionItemImageUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionItems", "ImageUrl", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionItems", "ImageUrl");
        }
    }
}
