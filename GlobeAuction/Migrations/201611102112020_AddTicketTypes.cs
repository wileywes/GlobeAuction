namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTicketTypes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TicketTypes",
                c => new
                    {
                        TicketTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Price = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                    })
                .PrimaryKey(t => t.TicketTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TicketTypes");
        }
    }
}
