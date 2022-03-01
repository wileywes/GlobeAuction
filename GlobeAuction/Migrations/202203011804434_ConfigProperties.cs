namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigProperties : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConfigProperties",
                c => new
                    {
                        ConfigPropertyId = c.Int(nullable: false, identity: true),
                        PropertyName = c.String(nullable: false),
                        PropertyValue = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                    })
                .PrimaryKey(t => t.ConfigPropertyId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ConfigProperties");
        }
    }
}
