namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRaffleTicketFlagToStoreItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StoreItems", "Quantity", c => c.Int(nullable: false));
            AddColumn("dbo.StoreItems", "IsRaffleTicket", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StoreItems", "IsRaffleTicket");
            DropColumn("dbo.StoreItems", "Quantity");
        }
    }
}
