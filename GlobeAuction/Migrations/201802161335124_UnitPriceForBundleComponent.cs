namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnitPriceForBundleComponent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BundleComponents", "ComponentUnitPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BundleComponents", "ComponentUnitPrice");
        }
    }
}
