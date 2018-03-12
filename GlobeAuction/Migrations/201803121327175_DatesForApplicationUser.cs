namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DatesForApplicationUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CreateDate", c => c.DateTime(nullable: true));
            Sql("UPDATE [dbo].[AspNetUsers] SET CreateDate=GETDATE()");
            AlterColumn("dbo.AspNetUsers", "CreateDate", c => c.DateTime(nullable: false));

            AddColumn("dbo.AspNetUsers", "LastLogin", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "LastLogin");
            DropColumn("dbo.AspNetUsers", "CreateDate");
        }
    }
}
