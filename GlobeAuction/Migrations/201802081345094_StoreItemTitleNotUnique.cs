namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoreItemTitleNotUnique : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.StoreItems", "IX_StoreItem_Title");
        }
        
        public override void Down()
        {
            CreateIndex("dbo.StoreItems", "Title", unique: true, name: "IX_StoreItem_Title");
        }
    }
}
