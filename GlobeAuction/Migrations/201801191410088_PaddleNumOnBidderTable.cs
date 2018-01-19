namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PaddleNumOnBidderTable : DbMigration
    {
        string indexName = "IX_UQ_Bidders_PaddleNum";
        string tableName = "dbo.Bidders";
        string columnName = "PaddleNumber";

        public override void Up()
        {
            AddColumn(tableName, columnName, c => c.Int());
            Sql(string.Format(@"
                CREATE UNIQUE NONCLUSTERED INDEX {0}
                ON {1}({2}) 
                WHERE {2} IS NOT NULL;",
                indexName, tableName, columnName));
        }
        
        public override void Down()
        {
            DropIndex(tableName, indexName);
            DropColumn(tableName, columnName);
        }
    }
}
