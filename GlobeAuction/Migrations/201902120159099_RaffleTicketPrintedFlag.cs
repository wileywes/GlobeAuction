namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RaffleTicketPrintedFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StoreItemPurchases", "IsRafflePrinted", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[StoreItemPurchases] SET IsRafflePrinted = 0");
            AlterColumn("dbo.StoreItemPurchases", "IsRafflePrinted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StoreItemPurchases", "IsRafflePrinted");
        }
    }
}
