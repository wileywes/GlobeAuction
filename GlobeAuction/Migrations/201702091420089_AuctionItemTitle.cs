namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuctionItemTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionItems", "Title", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionItems", "Title");
        }
    }
}
