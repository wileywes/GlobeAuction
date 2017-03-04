namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeStoreItemPriceToDecimal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StoreItems", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StoreItems", "Price", c => c.Int(nullable: false));
        }
    }
}
