namespace GlobeAuction.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class BundleStoreItemsAndRecordPriceWithSip : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BundleComponents",
                c => new
                    {
                        BundleComponentId = c.Int(nullable: false, identity: true),
                        StoreItemId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BundleComponentId)
                .ForeignKey("dbo.StoreItems", t => t.StoreItemId, cascadeDelete: true)
                .Index(t => t.StoreItemId);
            
            AddColumn("dbo.StoreItemPurchases", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.StoreItems", "IsBundleParent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BundleComponents", "StoreItemId", "dbo.StoreItems");
            DropIndex("dbo.BundleComponents", new[] { "StoreItemId" });
            DropColumn("dbo.StoreItems", "IsBundleParent");
            DropColumn("dbo.StoreItemPurchases", "Price");
            DropTable("dbo.BundleComponents");
        }
    }
}
