namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TicketTypePromoCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TicketTypes", "PromoCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TicketTypes", "PromoCode");
        }
    }
}
