namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DonorTaxReceiptFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Donors", "HasTaxReceiptBeenEmailed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Donors", "HasTaxReceiptBeenEmailed");
        }
    }
}
