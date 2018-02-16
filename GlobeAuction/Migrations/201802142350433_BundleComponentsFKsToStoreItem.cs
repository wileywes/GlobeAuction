namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BundleComponentsFKsToStoreItem : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BundleComponents", "StoreItemId", "dbo.StoreItems");
            DropIndex("dbo.BundleComponents", new[] { "StoreItemId" });
            AddColumn("dbo.BundleComponents", "BundleParent_StoreItemId", c => c.Int(nullable: false));
            AddColumn("dbo.BundleComponents", "StoreItem_StoreItemId", c => c.Int(nullable: false));
            CreateIndex("dbo.BundleComponents", "BundleParent_StoreItemId");
            CreateIndex("dbo.BundleComponents", "StoreItem_StoreItemId");
            AddForeignKey("dbo.BundleComponents", "StoreItem_StoreItemId", "dbo.StoreItems", "StoreItemId");
            AddForeignKey("dbo.BundleComponents", "BundleParent_StoreItemId", "dbo.StoreItems", "StoreItemId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BundleComponents", "BundleParent_StoreItemId", "dbo.StoreItems");
            DropForeignKey("dbo.BundleComponents", "StoreItem_StoreItemId", "dbo.StoreItems");
            DropIndex("dbo.BundleComponents", new[] { "StoreItem_StoreItemId" });
            DropIndex("dbo.BundleComponents", new[] { "BundleParent_StoreItemId" });
            DropColumn("dbo.BundleComponents", "StoreItem_StoreItemId");
            DropColumn("dbo.BundleComponents", "BundleParent_StoreItemId");
            CreateIndex("dbo.BundleComponents", "StoreItemId");
            AddForeignKey("dbo.BundleComponents", "StoreItemId", "dbo.StoreItems", "StoreItemId", cascadeDelete: true);
        }
    }
}
