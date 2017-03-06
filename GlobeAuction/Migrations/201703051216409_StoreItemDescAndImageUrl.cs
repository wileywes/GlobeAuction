namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoreItemDescAndImageUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StoreItems", "Description", c => c.String(nullable: false, defaultValue:"Description"));
            AddColumn("dbo.StoreItems", "ImageUrl", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StoreItems", "ImageUrl");
            DropColumn("dbo.StoreItems", "Description");
        }
    }
}
