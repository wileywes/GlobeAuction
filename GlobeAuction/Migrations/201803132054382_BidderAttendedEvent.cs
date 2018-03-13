namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidderAttendedEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bidders", "AttendedEvent", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[Bidders] SET AttendedEvent = 0");
            AlterColumn("dbo.Bidders", "AttendedEvent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bidders", "AttendedEvent");
        }
    }
}
