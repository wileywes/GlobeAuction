namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoftDeleteBidders : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Invoices", "Bidder_BidderId", "dbo.Bidders");
            DropIndex("dbo.Invoices", new[] { "Bidder_BidderId" });
            AddColumn("dbo.Bidders", "IsDeleted", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Invoices", "Bidder_BidderId", c => c.Int());
            CreateIndex("dbo.Invoices", "Bidder_BidderId");
            AddForeignKey("dbo.Invoices", "Bidder_BidderId", "dbo.Bidders", "BidderId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Invoices", "Bidder_BidderId", "dbo.Bidders");
            DropIndex("dbo.Invoices", new[] { "Bidder_BidderId" });
            AlterColumn("dbo.Invoices", "Bidder_BidderId", c => c.Int(nullable: false));
            DropColumn("dbo.Bidders", "IsDeleted");
            CreateIndex("dbo.Invoices", "Bidder_BidderId");
            AddForeignKey("dbo.Invoices", "Bidder_BidderId", "dbo.Bidders", "BidderId", cascadeDelete: true);
        }
    }
}
