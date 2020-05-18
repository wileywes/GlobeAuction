namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuctionItemInFiresale : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionItems", "IsInFiresale", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[AuctionItems] SET IsInFiresale = 0");
            AlterColumn("dbo.AuctionItems", "IsInFiresale", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionItems", "IsInFiresale");
        }
    }
}
