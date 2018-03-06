namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UseDigitalCertificateForWinnerOnDonationItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "UseDigitalCertificateForWinner", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "UseDigitalCertificateForWinner");
        }
    }
}
