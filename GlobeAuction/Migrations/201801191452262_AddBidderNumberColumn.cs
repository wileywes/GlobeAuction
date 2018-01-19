namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBidderNumberColumn : DbMigration
    {
        public override void Up()
        {
            //nullable first so we can set a value for existing records
            AddColumn("dbo.Bidders", "BidderNumber", c => c.Int(nullable: true));
            Sql("UPDATE [dbo].[Bidders] SET BidderNumber = 500 + BidderId");
            AlterColumn("dbo.Bidders", "BidderNumber", c => c.Int(nullable: false));

            CreateIndex("dbo.Bidders", "BidderNumber", unique: true, name: "IX_Bidder_BidderNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Bidders", "IX_Bidder_BidderNumber");
            DropColumn("dbo.Bidders", "BidderNumber");
        }
    }
}
