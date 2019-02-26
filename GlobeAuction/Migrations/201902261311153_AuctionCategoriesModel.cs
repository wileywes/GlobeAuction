namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuctionCategoriesModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuctionCategories",
                c => new
                    {
                        AuctionCategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        BidOpenDateLtz = c.DateTime(nullable: false),
                        BidCloseDateLtz = c.DateTime(nullable: false),
                        IsFundAProject = c.Boolean(nullable: false),
                        IsOnlyAvailableToAuctionItems = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.AuctionCategoryId);

            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Gift Card Grab', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Experiences and Entertainment', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Getaways', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Health Beauty and Fitness', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Children''s Corner', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Home Goods and Services', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Teacher Treasure', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('GLOBE Perks', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Class Art', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Fund-a-Project', '2019-12-31', '2019-12-31', 1, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Food Donations', '2019-12-31', '2019-12-31', 0, 0)");
            Sql("INSERT INTO [dbo].[AuctionCategories] (Name, BidOpenDateLtz, BidCloseDateLtz, IsFundAProject, IsOnlyAvailableToAuctionItems) VALUES ('Live', '2019-12-31', '2019-12-31', 0, 1)");


            AddColumn("dbo.AuctionItems", "Category_AuctionCategoryId", c => c.Int(nullable: false));
            AddColumn("dbo.DonationItems", "Category_AuctionCategoryId", c => c.Int(nullable: false));

            Sql("UPDATE ai SET Category_AuctionCategoryId = ac.AuctionCategoryId FROM AuctionItems ai inner join AuctionCategories ac on ac.Name = ai.Category");
            Sql("UPDATE di SET Category_AuctionCategoryId = ac.AuctionCategoryId FROM DonationItems di inner join AuctionCategories ac on ac.Name = di.Category");

            CreateIndex("dbo.AuctionItems", "Category_AuctionCategoryId");
            CreateIndex("dbo.DonationItems", "Category_AuctionCategoryId");
            AddForeignKey("dbo.AuctionItems", "Category_AuctionCategoryId", "dbo.AuctionCategories", "AuctionCategoryId", cascadeDelete: false);
            AddForeignKey("dbo.DonationItems", "Category_AuctionCategoryId", "dbo.AuctionCategories", "AuctionCategoryId", cascadeDelete: false);
            DropColumn("dbo.AuctionItems", "Category");
            DropColumn("dbo.DonationItems", "Category");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DonationItems", "Category", c => c.String(nullable: false));
            AddColumn("dbo.AuctionItems", "Category", c => c.String(nullable: false));
            DropForeignKey("dbo.DonationItems", "Category_AuctionCategoryId", "dbo.AuctionCategories");
            DropForeignKey("dbo.AuctionItems", "Category_AuctionCategoryId", "dbo.AuctionCategories");
            DropIndex("dbo.DonationItems", new[] { "Category_AuctionCategoryId" });
            DropIndex("dbo.AuctionItems", new[] { "Category_AuctionCategoryId" });
            DropColumn("dbo.DonationItems", "Category_AuctionCategoryId");
            DropColumn("dbo.AuctionItems", "Category_AuctionCategoryId");
            DropTable("dbo.AuctionCategories");
        }
    }
}
