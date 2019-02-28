namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CategoryReferenceNullableOnItems : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AuctionItems", "Category_AuctionCategoryId", "dbo.AuctionCategories");
            DropForeignKey("dbo.DonationItems", "Category_AuctionCategoryId", "dbo.AuctionCategories");
            DropIndex("dbo.AuctionItems", new[] { "Category_AuctionCategoryId" });
            DropIndex("dbo.DonationItems", new[] { "Category_AuctionCategoryId" });
            AlterColumn("dbo.AuctionItems", "Category_AuctionCategoryId", c => c.Int());
            AlterColumn("dbo.DonationItems", "Category_AuctionCategoryId", c => c.Int());
            CreateIndex("dbo.AuctionItems", "Category_AuctionCategoryId");
            CreateIndex("dbo.DonationItems", "Category_AuctionCategoryId");
            AddForeignKey("dbo.AuctionItems", "Category_AuctionCategoryId", "dbo.AuctionCategories", "AuctionCategoryId");
            AddForeignKey("dbo.DonationItems", "Category_AuctionCategoryId", "dbo.AuctionCategories", "AuctionCategoryId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DonationItems", "Category_AuctionCategoryId", "dbo.AuctionCategories");
            DropForeignKey("dbo.AuctionItems", "Category_AuctionCategoryId", "dbo.AuctionCategories");
            DropIndex("dbo.DonationItems", new[] { "Category_AuctionCategoryId" });
            DropIndex("dbo.AuctionItems", new[] { "Category_AuctionCategoryId" });
            AlterColumn("dbo.DonationItems", "Category_AuctionCategoryId", c => c.Int(nullable: false));
            AlterColumn("dbo.AuctionItems", "Category_AuctionCategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.DonationItems", "Category_AuctionCategoryId");
            CreateIndex("dbo.AuctionItems", "Category_AuctionCategoryId");
            AddForeignKey("dbo.DonationItems", "Category_AuctionCategoryId", "dbo.AuctionCategories", "AuctionCategoryId", cascadeDelete: true);
            AddForeignKey("dbo.AuctionItems", "Category_AuctionCategoryId", "dbo.AuctionCategories", "AuctionCategoryId", cascadeDelete: true);
        }
    }
}
