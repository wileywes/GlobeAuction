namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TicketsOnlyVisibleToAdmins : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TicketTypes", "OnlyVisibleToAdmins", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TicketTypes", "OnlyVisibleToAdmins");
        }
    }
}
