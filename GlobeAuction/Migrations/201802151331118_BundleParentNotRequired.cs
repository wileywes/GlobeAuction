namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BundleParentNotRequired : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BundleComponents", "BundleParent_StoreItemId", "dbo.StoreItems");
            DropIndex("dbo.BundleComponents", new[] { "BundleParent_StoreItemId" });
            AlterColumn("dbo.BundleComponents", "BundleParent_StoreItemId", c => c.Int());
            CreateIndex("dbo.BundleComponents", "BundleParent_StoreItemId");
            AddForeignKey("dbo.BundleComponents", "BundleParent_StoreItemId", "dbo.StoreItems", "StoreItemId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BundleComponents", "BundleParent_StoreItemId", "dbo.StoreItems");
            DropIndex("dbo.BundleComponents", new[] { "BundleParent_StoreItemId" });
            AlterColumn("dbo.BundleComponents", "BundleParent_StoreItemId", c => c.Int(nullable: false));
            CreateIndex("dbo.BundleComponents", "BundleParent_StoreItemId");
            AddForeignKey("dbo.BundleComponents", "BundleParent_StoreItemId", "dbo.StoreItems", "StoreItemId", cascadeDelete: true);
        }
    }
}
