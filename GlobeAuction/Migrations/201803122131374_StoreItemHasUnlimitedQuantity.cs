namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoreItemHasUnlimitedQuantity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StoreItems", "HasUnlimitedQuantity", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[StoreItems] SET HasUnlimitedQuantity = IsRaffleTicket");
            AlterColumn("dbo.StoreItems", "HasUnlimitedQuantity", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StoreItems", "HasUnlimitedQuantity");
        }
    }
}
