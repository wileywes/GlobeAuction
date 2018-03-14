namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuctionItemMasterItemForMultiWinner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionItems", "IsMasterItemForMultipleWinners", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[AuctionItems] SET IsMasterItemForMultipleWinners = 0");
            AlterColumn("dbo.AuctionItems", "IsMasterItemForMultipleWinners", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionItems", "IsMasterItemForMultipleWinners");
        }
    }
}
