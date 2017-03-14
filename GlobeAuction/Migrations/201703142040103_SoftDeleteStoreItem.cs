namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoftDeleteStoreItem : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StoreItemPurchases", "StoreItem_StoreItemId", "dbo.StoreItems");
            AddColumn("dbo.StoreItems", "IsDeleted", c => c.Boolean(nullable: false));
            AddForeignKey("dbo.StoreItemPurchases", "StoreItem_StoreItemId", "dbo.StoreItems", "StoreItemId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StoreItemPurchases", "StoreItem_StoreItemId", "dbo.StoreItems");
            DropColumn("dbo.StoreItems", "IsDeleted");
            AddForeignKey("dbo.StoreItemPurchases", "StoreItem_StoreItemId", "dbo.StoreItems", "StoreItemId", cascadeDelete: true);
        }
    }
}
