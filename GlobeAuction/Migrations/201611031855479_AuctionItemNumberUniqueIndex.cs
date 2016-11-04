namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuctionItemNumberUniqueIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.AuctionItems", "UniqueItemNumber", unique: true, name: "IX_AuctionItem_UniqueItemNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AuctionItems", "IX_AuctionItem_UniqueItemNumber");
        }
    }
}
