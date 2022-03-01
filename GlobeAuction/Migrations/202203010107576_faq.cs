namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class faq : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FaqCategories",
                c => new
                    {
                        FaqCategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FaqCategoryId);
            
            CreateTable(
                "dbo.Faqs",
                c => new
                    {
                        FaqId = c.Int(nullable: false, identity: true),
                        Question = c.String(nullable: false),
                        Answer = c.String(),
                        OrderInCategory = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                        Category_FaqCategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FaqId)
                .ForeignKey("dbo.FaqCategories", t => t.Category_FaqCategoryId, cascadeDelete: true)
                .Index(t => t.Category_FaqCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Faqs", "Category_FaqCategoryId", "dbo.FaqCategories");
            DropIndex("dbo.Faqs", new[] { "Category_FaqCategoryId" });
            DropTable("dbo.Faqs");
            DropTable("dbo.FaqCategories");
        }
    }
}
