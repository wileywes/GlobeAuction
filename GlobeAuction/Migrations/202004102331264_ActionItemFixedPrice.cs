namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionItemFixedPrice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionItems", "IsFixedPrice", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[AuctionItems] SET IsFixedPrice = 0");
            AlterColumn("dbo.AuctionItems", "IsFixedPrice", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionItems", "IsFixedPrice");
        }
    }
}
