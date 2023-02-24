namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShoutOutsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ShoutOuts",
                c => new
                    {
                        ShoutOutId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        BodyText = c.String(nullable: false),
                        ImageUrl = c.String(maxLength: 500),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                    })
                .PrimaryKey(t => t.ShoutOutId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ShoutOuts");
        }
    }
}
